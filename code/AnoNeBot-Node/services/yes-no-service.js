//@ts-check
const req = require("request-promise")

module.exports.get = (translate = false) => {
    return new Promise((resolve, reject) => {
        req.get("https://yesno.wtf/api/").then((resp) => {
            let r = JSON.parse(resp);
            if (translate)
                r.answer = _translate(r.answer);
            resolve(r);
        }).catch((error) => {
            console.log(`Error calling YesNo API. ${error}`);
            reject(error);
        });
    });
}

function _translate(yesNo) {
    let translation;

    switch (yesNo.toLowerCase()) {
        case "yes":
            translation = "Ano";
            break;
        case "no":
            translation = "Ne";
            break;
        default:
            translation = "Nejsem si jistý";
            break;
    }

    return translation;
}