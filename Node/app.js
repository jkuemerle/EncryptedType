const crypto = require('crypto');
const fs = require('fs');
const constants = require('./encryptedTypeConstants.js'); 
const mockks = require('./mockKeyServer.js');
var ks = [ mockks ];

var enc = "Key2~AES~HS256~H669DtnhM7uEyjnF/HszYQ==~pMTKFKk5nT5K8XH2PxAuMlBumE56qTpxmbac2sp1Z5wcj60HhDMVmKC2RKumGzh5aCi7mTXmSF5NjFGvXfwVFw==~WTP35Q09qw8U6/aYq1/z3xR7X7Bv8yxRIi2k4fgPf/M=";
var integrity = "123456";
var dec = Decrypt(ks,enc,integrity);
console.log(dec);    

var enc = Encrypt("foo",integrity,ks,"Key2");
console.log(enc);

var dec2 = Decrypt(ks,enc,integrity);
console.log(dec2);


function Encrypt(data,integrity,keyServers,keyID, hmacID, crypterID)
{
    if(typeof hmacID === "undefined")
    {
        hmacID = constants.HS256;
    }
    if(typeof crypterID === "undefined")
    {
        crypterID = constants.AES;
    }    
    var crypter = GetCrypterProps(crypterID);
    // generate random iv
    var iv = crypto.randomBytes(crypter.ivsize);
    var keyInfo = GetKeyInfo(keyServers,keyID,iv,crypter);
    crypter = GetEncrypter(crypter,crypterID,keyInfo,iv);
    var workData = data;
    if(integrity != null )
    {
        var hv = hashValue(hmacID,data,integrity).toString('base64');
        workData += "\0" + hv;
    }
    var rawencryptWork = crypter.crypter.update(workData,'utf8');
    var rawencryptFinal = crypter.crypter.final();
    var rawencrypt = Buffer.concat([rawencryptWork,rawencryptFinal]);
    var rawhmac = hashValue(hmacID,rawencrypt,keyInfo.secret);
    var ret = keyID + "~" + crypterID + "~" + hmacID + "~" + iv.toString('base64') + "~" + rawencrypt.toString('base64') + "~" + rawhmac.toString('base64');
    return ret;
}

function Decrypt(keyServers,enc,integrity)
{
    var fields = enc.split('~');
    var keyID = fields[0];
    var crypterID = fields[1];
    var hmacID = fields[2];
    var ivBase64 = fields[3];
    var dataBase64 = fields[4];
    var hmacBase64 = fields[5];
    var iv = Buffer.from(ivBase64,'base64');
    var data = Buffer.from(dataBase64,'base64');
    var hmac = Buffer.from(hmacBase64, 'base64');
    var crypter = GetCrypterProps(crypterID);
    var keyInfo = GetKeyInfo(keyServers,keyID,iv,crypter);
    crypter = GetDecrypter(crypter,crypterID,keyInfo,iv);
    // verify that hmac is valid
    if(!constantTimeCompare(hmac, hashValue(hmacID,data,keyInfo.secret)))
    {
        return null;
    }
    var rawdecrypt = crypter.crypter.update(data, 'hex', 'utf8');
    rawdecrypt += crypter.crypter.final('utf8');
    if(rawdecrypt.indexOf('\0') > 0 && (integrity == null || integrity.length === 0) )
    {
        // if there is integrity in the data and no integrity value we do not return data
        return null;
    }
    if(integrity == null || integrity.length === 0)
    {
        // there is no integrity check
        return rawdecrypt;
    }
    var encfields = rawdecrypt.split("\0");
    var decrypt = encfields[0];
    var dechmac = Buffer.from(encfields[1],'base64');
    var calchmac = hashValue(hmacID,decrypt,integrity);
    if(constantTimeCompare(dechmac,calchmac))
    {
        return decrypt;
    }
    else
    {
        return null;
    }
    return null;
}

function hashValue(HMACID, data, integrity)
{
    var hsh = getHasher(HMACID,integrity);
    var ret = hsh.update(data,'utf8').digest();
    //var ret = hsh.digest(Buffer.from(data,'utf8'));
    return ret;
} 

function getHasher(HMACID, integrity)
{
    var ret = null;
    switch(HMACID)
    {
        case constants.HS256 : 
            ret = crypto.createHmac('sha256', integrity);
            break;
    }
    return ret;
}

function GetKeyInfo(ks, keyID, iv, crypter)
{
    var keyText = null;
    if(ks)
    {
        if(!(ks.constructor === Array))
        {
            keyText = ks.GetKey(keyID);            
        } 
        else
        {
            for (var i = 0, len = ks.length; i < len; i++) {
                keyText = ks[i].GetKey(keyID);
                if(!keyText)
                {
                    break;
                }
            }
        }
    }
    // matches Rfc2898DerivedBytes to generate key values
    var derivedKey = crypto.pbkdf2Sync(keyText, iv, 1000, 256, 'sha1');
    // first 32 bytes is key, next 32 is secret value
    var key = derivedKey.slice(0,crypter.keysize);
    var sec = derivedKey.slice(crypter.keysize,crypter.keysize + crypter.secretsize);
    var ret = { key : key, secret : sec }; 
    return ret;
}

function GetCrypterProps(crypterID)
{
    var ret = null;
    switch(crypterID)
    {
        case constants.AES :
            ret = { crypter : null, keysize : 32, secretsize : 32, ivsize : 16 };
            break;
    }
    return ret;
}

function GetDecrypter(crypter, crypterID, keyinfo, iv)
{
    switch(crypterID)
    {
        case constants.AES :
            crypter.crypter = crypto.createDecipheriv('aes-256-cbc',keyinfo.key,iv);
            break;
    }
    return crypter;
}

function GetEncrypter(crypter, crypterID, keyinfo, iv)
{
    switch(crypterID)
    {
        case constants.AES :
            crypter.crypter = crypto.createCipheriv('aes-256-cbc',keyinfo.key,iv);
            break;
    }
    return crypter;
}

function constantTimeCompare(first,second)
{
    var result = true;
    if(first.length != second.length)
    {
        result = false;
    }
    var max = first.length > second.length ? second.length : first.length;
    for(i = 0; i < max; i++)
    {
        if(first[i] != second[i])
        {
            result = false;
        }
    }
    return result;
}
