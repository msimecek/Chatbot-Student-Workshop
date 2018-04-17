---

typora-copy-images-to: images
---

**This is the second variant of the original YesNo workshop, adjusted for Node.js.**

In this workshop, you will learn how to create your own chatbot using  **Visual Studio Code**, **Node.js** and the **Microsoft Bot Framework**.

## Assumptions

There will be a lot of copy-pasting in this workshop, but it's better to have a basic understanding of how web applications work and are built.

For reading documentation, finding solutions to problems in development and programming in general it is better to know **English**. This workshop is in English. Some of the screen captures are Czech as it was originally designed by [@msimecek](https://github.com/msimecek).

## Output

At the end of this exercise you will have a chatbot with two functionalities:

*Answer Yes/No to any question*

![1518006751354](images/1518006751354.png)

*Guessing names by face*

![1518107478717](images/1518107478717.png)

## Preparation

[Download](https://code.visualstudio.com/) and install **Visual Studio Code**. It's free and open-source.

[Download](https://github.com/Microsoft/BotFramework-Emulator/releases) and install the **Bot Framework Emulator**. Select the current version, *Setup ... exe*:

![1517995547064](images/1517995547064.png)

## First Bot - Yes/No?

In the first part you will learn the basic principles of creating a chatbot and structuring the code.

1. Create a folder where you'll put all your code.

2. Run **Visual Studio Code** and open this folder.

3. Initialize NPM by opening the Terminal and typing `npm init`.

   > **Hint**: In VS Code press F1, type "toggle terminal" and select **View: Toggle Integrated Terminal**.
   >
   > ![1523972001382](images/1523972001382.png)

4. Go through the questions until the `package.json` file is created.

5. In the same folder create a new file, call it `yesnobot.js`.

We are going to start with just a "smoke test", to check that our bot responds and everything is set up properly.

Since chatbot is just a web API, we're going to need a web server. Bot Framework is compatible with *Restify* and *Express*, so let's use the former.

Get back to your **terminal** and **install package**:

```
npm install --save restify
```

Then install the Bot Builder SDK:

```
npm install --save botbuilder
```

And finally, one more package we will make use of later:

```
npm install --save request-promise
```

With that we're ready to start coding the bot. Go to **yesnobot.js** and put in the basics:

```javascript
//@ts-check
const restify = require('restify');
const builder = require('botbuilder');

const server = restify.createServer();
server.listen(process.env.port || process.env.PORT || 3979, function() {
    console.log("%s listening to %s", server.name, server.url);
});

const connector = new builder.ChatConnector({
    appId: process.env["MicrosoftAppId"],
    appPassword: process.env["MicrosoftAppPassword"]
});

server.post("/api/messages", connector.listen());
```

The server will listen for POST requests coming to `/api/messages` at the port 3979 (if not specified otherwise).

> **Hint**: VS Code comes with native support for TypeScript, but even if you prefer pure JavaScript, it's still nice to turn the type checking on by adding `//@ts-check` on top of every JS file.

Now add some simple functionality:

```javascript
const bot = new builder.UniversalBot(connector, function(session) {
    session.send(`You said ${session.message.text} which was ${session.message.text.length} characters`); 
});
```

You can now start the application using the **F5** key (or the green arrow "Play" button). A browser window opens. Go to **Bot Framework Emulator**, click on the box with the text **Enter your endpoint URL** and select your server address and port (in this case it should be *localhost:3979*):

![1518003750998](images/1518003750998.png)

The full address is `http://localhost:3979/api/messages`.

Leave the **Microsoft App ID** and **Microsoft App Password** fields blank and click **Connect**.

If you type something now, the bot will answer:

![1518003927427](images/1518003927427.png)

Good, we have checked that everything is working fine and can jump into the "serious" code.

### UniversalBot

In Visual Studio Code, stop Debugging (**Shift + F5** or the  "Stop" button):

![1523973222616](images/1523973222616.png)

Delete the *waterfall* implementation from UniversalBot and keep it like this:

```javascript
const bot = new builder.UniversalBot(connector);
```

There's still a little bit of supporting functionality missing, so we're going to add a service.

### Services

This bot will be "smarter" (funnier) and use an external service. At http://yesno.wtf, there is a public API that randomly returns a "yes" or "no" and an appropriate GIF. Our bot, the consultant, will use it to answer any question. At the end of the day, you don't need anything than yes or no from a consultant :-)

![1518006751354](images/1518006751354.png)

First, prepare the *yes-no-service*:

1. Add new folder to the project by clicking the icon in VS Code. Name it **services**.

   ![1523973510760](images/1523973510760.png)

2. Add new file to this folder and call it **yes-no-service.js**.

   ![1523973573916](images/1523973573916.png)

3. Complete the implementation:

```javascript
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
            translation = "Yes";
            break;
        case "no":
            translation = "No";
            break;
        default:
            translation = "I'm not sure";
            break;
    }

    return translation;
}
```
In this code, you can translate the answer into any language, just replace, the "*Yes*", "*No*", "*Maybe*" by any translation.


What's happening here?

Using the *request-promise* library, we will request a response from *YesNo API*.
* The received JSON string is parsed into a JavaScript object.

  ```javascript
  {"answer":"yes","forced":false,"image":"https://yesno.wtf/assets/yes/6-304e564038051dab8a5aa43156cdc20d.gif"}
  ```

* Optionally, the response gets translated.

* The completed response is returned for further processing.

### Root dialog - basic implementation

We will build only a single dialog for this bot and place it into a single file. In reality, you could have multiple dialogs in multiple files, calling each other and switching context.

Go back to **yesnotbot.js** and adjust the code to take advantage of the newly prepared service. What do we want the bot to do?

* Receive a message from the user.
* Check if it is a question.
* Request a Yes/No response from yes-no-service.
* Send this reply back to the user.

At the end of the file add the following:

```javascript
bot.dialog("/", [
    function(session) {
        if (!session.message.text.endsWith("?")) {
            session.send("That doesn't seem like a question.");
        }
        else {
            YesNoService.get(true).then((resp) => {
                session.send(resp.answer);
            });
        }
    }
]);
```

`YesNoService` will be underlined in red. Therefore, add at the beginning of the file (below `const builder...`) reference to our new service:

```javascript
const YesNoService = require('./services/yes-no-service');
```

Run the application (**F5**), go to **Bot Framework Emulator** and try to ask a question.


![1518009001857](images/1518009001857.png)

### Root dialog - with pictures

Bot Framework allows you to take advantage of graphical elements available on different chatbot channels. We will use the so-called *HeroCard* and in addition to the blunt Yes/No reply we send the user an animated GIF.

Replace the root dialog code with this one, using the Hero Card:

```javascript
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
```

And implement the function which generates this card:

```javascript
function createHeroCard(session, title, text, imageUrl) {
    return new builder.HeroCard(session)
            .title(title)
            .text(text)
            .images([builder.CardImage.create(session, imageUrl)]);
}
```

When you start the app now and ask the bot a question, you get a much richer response.

![1518009627918](images/1518009627918.png)

## Second Bot - who is it?

In the second exercise, we add a new dialog to Chatbota and we will show you how to work with status user information. This extension will help remembering the names of new people-Bot will offer a photo and the user would have to guess the name of a person.

### Preparation

We will develop an already created project, so there is no need to create a new.

1. Create a new folder in the project, name it **Assets**.

2. Get photos of people you want to learn and paste into the **Assets** Folder (right-click in Visual Studio > **Add > existing Item...**).

   ![1518100369875](images/1518100369875.png)

3. Add a new class to the **Models** folder. Name it **PeopleModel** (Add > Class... > PeopleModel.cs).

4. Complete the implementation (replace the values with your file names and name, or add more lines):

```c#
public class PeopleModel
{
    public static Dictionary<string, string> People = new Dictionary<string, string>()
    {
        { "Assets/jarda.jpg", "Jarda" },
        { "Assets/martin.jpg", "Martin" },
        { "Assets/satya.jpg", "Satya" }
    };
}
```

5. Add a using at the beginning of the file:

```c#
using System.Collections.Generic;
```

So we're done for data source preparation. Bot will draw from the list of `People` and will randomly send pictures and check the correctness of the name (first and second value). We'll wrap this whole functionality into a new dialogue.

### WhoIsDialog

Create a new file type **Bot Dialog** in the folder **dialogs** and file name **WhoIsDialog.cs** and insert the implementation of the `MessageReceivedAsync()` method:

```c#
private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
{
    var activity = await result as IMessageActivity;

    if (context.ConversationData.ContainsKey("LastFace"))
    {
        var lastFace = context.ConversationData.GetValue<KeyValuePair<string, string>>("LastFace");
            
        if (activity.Text.ToLower() == lastFace.Value.ToLower())
        {
            await context.PostAsync("Correct!");
        }
        else
        {
            await context.PostAsync("Error! It's " + lastFace.Value);
        }
    }

    await ShowRandomFaceAsync(context);

    context.Wait(MessageReceivedAsync);
}
```

Add the missing using at the beginning of the file:

```c#
using System.Collections.Generic;
```

Whenever a message comes from the user, we will first look to see if we have asked him about the name of the face. If so, we will pull out the information of this face (i.e., image and name).

```c#
var lastFace = context.ConversationData.GetValue<KeyValuePair<string, string>>("LastFace");
```

And then we compare what came in the message with the name of the face.

```c#
if (activity.Text.ToLower() == lastFace.Value.ToLower())
```

> In our case `lastFace.Value` contains the name and `lastFace.Key` the picture.

Depending on the evaluation, we will send the user an appropriate response.

At the end we will send a new face and wait for the answer again.

In order for the bot to function, it remains below in the *MessageReceivedAsync()* to add a method `ShowRandomFaceAsync()`.

### Show Random Face

In this helper method we want to randomly select one from the list of faces, assemble the ImageCard and send it to the user.

```c#
private async Task ShowRandomFaceAsync(IDialogContext context)
{
    Random rand = new Random();
    var face = PeopleModel.People.ElementAt(rand.Next(0, PeopleModel.People.Count));

    context.ConversationData.SetValue("LastFace", face);

    var root = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/";
    var card = new ThumbnailCard("Who is it?", images: new List<CardImage>() { new CardImage(root + face.Key) });

    var message = context.MakeMessage();
    message.Attachments.Add(card.ToAttachment());

    await context.PostAsync(message);
}
```

Add the missing using at the beginning of the file:

```c#
using AnoNeBot.Models;
using System.Linq;
using System.Web;
```

In the previous exercises we used `HeroCard`, now we are trying `ThumbnailCard`, which has a slightly different layout.

> For an overview of all card types, see [documentation](https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-send-rich-cards).

Two elements are key in this method. After selecting a random face, we save it in `ConversationData`, because when the user writes an answer, we will want to check if it is correct (we already have it in the code above).

```c#
context.ConversationData.SetValue("LastFace", face);
```

The second important element is the generation of a "card" with a photo. The principle is the same as in the previous exercise, however here we need to compile the web address of the image dynamically because you we host it on the webserver yourself.

```c#
var root = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/";
...
new CardImage(root + face.Key) // face.Key => "Assets/martin.jpg"
```

Then we send the result to the user.

Note that `context` is being forwarded to the method.

The whole `WhoIsDialog` should look like this:

```c#
[Serializable]
public class WhoIsDialog : IDialog<object>
{
    public Task StartAsync(IDialogContext context)
    {
        context.Wait(MessageReceivedAsync);

        return Task.CompletedTask;
    }

    private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
    {
        var activity = await result as IMessageActivity;

        if (context.ConversationData.ContainsKey("LastFace"))
        {
            var lastFace = context.ConversationData.GetValue<KeyValuePair<string, string>>("LastFace");

            if (activity.Text.ToLower() == lastFace.Value.ToLower())
            {
                await context.PostAsync("Correct!");
            }
            else
            {
                await context.PostAsync("Error! It's  " + lastFace.Value);
            }
        }

        await ShowRandomFaceAsync(context);

        context.Wait(MessageReceivedAsync);
    }

    private async Task ShowRandomFaceAsync(IDialogContext context)
    {
        Random rand = new Random();
        var face = PeopleModel.People.ElementAt(rand.Next(0, PeopleModel.People.Count));

        context.ConversationData.SetValue("LastFace", face);

        var root = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/";
        var card = new ThumbnailCard("Who is it?", images: new List<CardImage>() { new CardImage(root + face.Key) });

        var message = context.MakeMessage();
        message.Attachments.Add(card.ToAttachment());

        await context.PostAsync(message);
    }
}
```

### Changing the dialog

Before you try the new dialog, you need to change the message routing in the **MessagesController.cs** So that the application uses the new WhoIsDialog instead of RootDialog.

```c#
public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
{
    if (activity.Type == ActivityTypes.Message)
    {
        //await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
        await Conversation.SendAsync(activity, () => new Dialogs.WhoIsDialog());
    }
    else
    {
        HandleSystemMessage(activity);
    }
    var response = Request.CreateResponse(HttpStatusCode.OK);
    return response;
}
```

If you run the application now and write a message to the chatbot, you should get a picture response:

![1518107478717](images/1518107478717.png)

## (Optional) Connection to actual channels

Chatting with Chatbot in the emulator is fun, but you can't offer it to other users in this way. If you want to tap into real channels (Skype, Messenger, etc.), you'll need to do a few more things.

To register a chatbot on the communication channels, you will need a **Microsoft Azure account**.

* [Trial](https://azure.microsoft.com/en-us/free/) It's free for the moon. You will receive a $100 credit for free use.
* [Dev Essentials](https://www.visualstudio.com/dev-essentials/) Includes a monthly renewal credit for one year.

Then, on [Microsoft Azure portal](https://portal.azure.com), you create a new source of type **Bot Channels Registration**.

![1518433351399](images/1518433351399.png)

Place it in the region **North Europe** and select the price level **F0**:

![1518433487612](images/1518433487612.png)

> So far leave the **Messaging endpoint** value empty, we will change it later.

Create a **Microsoft App ID and password** and select **Create New**. In the panel that opens, click **Create App ID in the App Registration Portal**.

![1518438690645](images/1518438690645.png)

Make a note of the generated **APP ID** (like in Notepad) and click the button. A password will appear **App Password** - also save it somewhere (when you click the popup, you can no longer get to it so really make sure you save it). Confirm and you can close this tab and return to the Azure portal.

Enter the newly collected data in the appropriate fields:

![1518439093805](images/1518439093805.png)

And also in Visual studio to a file **Web.config**:

```xml
<add key="BotId" value="mujbot" />
<add key="MicrosoftAppId" value="App ID here" />
<add key="MicrosoftAppPassword" value="App Password here" />
```

You can now complete the bot registration and confirm all open panels:

![1518439123488](images/1518439123488.png)

Click through your newly created Bot Service. For example, you'll see that in the **channels** section, you can choose which communication channels the bot will be available on.**Test in Web Chat** will be used to quickly try out the conversation (it won't work at the moment).

The chatbot code, the Web application that we created from scratch, must be accessible from the Internet. Therefore, you should deploy it to a Web server and obtain its HTTPS address. For testing, you can also reach the same effect directly from your computer using the [Ngrok](https://www.robinosborne.co.uk/2016/09/19/debugging-botframework-locally-using-ngrok/) tool.

> In practice, you would simply deploy the chabtot [for example, to Azure](https://almvm.azurewebsites.net/labs/vsts/appservice/).

To enter the address of the Web application with your Chatbot, go in the **Settings**, then to the field **Messaging endpoint**, and add `/api/messages` to the end:

![1518436343949](images/1518436343949.png)

Now when you run the app in Visual Studio and try to write to the bot in **Test in Web Chat**, it should start responding:

![1518440218267](images/1518440218267.png)

For Skype, add it in the **Channels** Section:

![1518440269608](images/1518440269608.png)

After saving, just click on Skype in the channel list and start chatting:

![1518440394854](images/1518440394854.png)

## Conclusion

In two sections, you learned how to create a simple chatbot in C#, how to send to a user a message enriched with images, and how to work with the state between messages.

Possible additional extensions:

* Use RootDialog as a signpost that will offer the user whether he wants to prefer to know the answer to the question or to learn the names.
* Ensure that the photos do not recur (i.e. not to show the same person several times until they are in the queue next).
* Load photos and people names dynamically, for example, from Office 365.
* Store the user states in your own table or SQL Database

## Additional Resources

* [Official documentation](https://docs.microsoft.com/en-us/bot-framework/)
* [Setup communication channels](https://docs.microsoft.com/en-us/bot-framework/bot-service-manage-channels)