exports.Keys = () =>  ["Key2"];

exports.GetKey = (KeyID) => {
    var ret = null;
    switch(KeyID)
    {
        case "Key2" :
            ret = "foo";
            break;

    }
    return ret;
};