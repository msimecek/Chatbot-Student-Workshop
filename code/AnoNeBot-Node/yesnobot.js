//@ts-check
const restify = require('restify');
const builder = require('botbuilder');
const YesNoService = require('./services/yes-no-service');

const server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3979, function() {
    console.log("%s listening to %s", server.name, server.url);
});

const connector = new builder.ChatConnector({
    appId: process.env["MicrosoftAppId"],
    appPassword: process.env["MicrosoftAppPassword"]
});

server.post("/api/messages", connector.listen());

const bot = new builder.UniversalBot(connector);

bot.dialog("/", [
    function(session) {
        if (!session.message.text.endsWith("?")) {
            session.send("That doesn't seem like a question.");
        }
        else {
            YesNoService.get(true).then((resp) => {
                var msg = new builder.Message(session).addAttachment(createHeroCard(session, resp.answer, "", resp.image));
                session.send(msg);
            });
        }
    }
]);

function createHeroCard(session, title, text, imageUrl) {
    return new builder.HeroCard(session)
            .title(title)
            .text(text)
            .images([builder.CardImage.create(session, imageUrl)]);
}