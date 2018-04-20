//@ts-check
const builder = require('botbuilder');
const fs = require('fs');
const PeopleService = require('./services/people-service')

module.exports = [
    function(session) {
        if (session.message.text === "back") {
            session.endDialog();
            return;
        }
        
        if (session.conversationData.lastFaceName !== undefined) {
            const lastFace = session.conversationData.lastFaceName;
            if (session.message.text.toLowerCase() === lastFace.toLowerCase()) {
                session.send("Správně!");
            }
            else {
                session.send("Incorrect! It's **" + lastFace + "**.");
            }
        }

        showRandomFace(session);
    }
];

function showRandomFace(session) {
    const faces = PeopleService.get();
    const rand = getRandomInt(faces.length);
    const face = faces[rand];
    const faceImage = "data:image/jpeg;base64," + base64_encode(face.image);

    session.conversationData.lastFaceName = face.name;
    var msg = new builder.Message(session).addAttachment(createThumbnailCard(session, "Kdo je to?", "", faceImage));
    session.send(msg);
}

function getRandomInt(max) {
    return Math.floor(Math.random() * Math.floor(max));
}

function createThumbnailCard(session, title, text, imageUrl) {
    return new builder.ThumbnailCard(session)
            .title(title)
            .text(text)
            .images([builder.CardImage.create(session, imageUrl)]);
}

function base64_encode(file) {
    var bitmap = fs.readFileSync(file);
    return new Buffer(bitmap).toString('base64');
}