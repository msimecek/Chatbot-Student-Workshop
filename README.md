---
typora-root-url: images
typora-copy-images-to: images
---

V tomto workshopu se naučíte, jak vytvořit vlastního chatbota pomocí nástroje **Visual Studio**

**2017**, jazyku **C#** a technologie **Microsoft Bot Framework**.

## Předpoklady

ABCD (znalost C# apod.)

## Výstup

= dva screenshoty + popis

## Příprava

[Stáhněte](https://www.visualstudio.com/) a nainstalujte si Visual Studio 2017. Bude stačit edice **Community**, která je pro studenty zdarma.

Při instalaci vyberte hlavně **ASP.NET and web development**:

![1517995366078](/1517995366078.png)

[Stáhněte](https://github.com/Microsoft/BotFramework-Emulator/releases) a nainstalujte si Bot Framework Emulator. Vyberte aktuální verzi, *Setup ... exe*:

![1517995547064](/1517995547064.png)

Stáhněte si šablony [projektu](http://aka.ms/bf-bc-vstemplate) a souborů ([controller](http://aka.ms/bf-bc-vscontrollertemplate), [dialog](http://aka.ms/bf-bc-vsdialogtemplate)) pro Visual Studio. ZIP soubory **nerozbalujte**, ale zkopírujte do složek Visual Studia.

* *Bot Application.zip* patří do `%USERPROFILE%\Documents\Visual Studio 2017\Templates\ProjectTemplates\Visual C#\`
* *Bot Controller.zip* a *Bot Dialog.zip* patří do `%USERPROFILE%\Documents\Visual Studio 2017\Templates\ItemTemplates\Visual C#\`

Spusťte **Visual Studio 2017** a ověřte, že v nabídce **New project** máte projekt typu **Bot Application**:

![1517995840433](/1517995840433.png)

Spusťte **Bot Framework Emulator**.

## Bot první - Ano/Ne?

V první části se seznámíte se základními principy tvorby chatbota a strukturování kódu.

1. Založte ve Visual Studiu 2017 nový projekt typu **Bot Application**.

2. Zvolte si název dle libosti, například "*AnoNeBot*".

   ![1517996913543](/1517996913543.png)

3. Vygeneruje se základní kostra chatbota.

4. Stiskněte klávesu **F6** (nebo vyberte v menu **Build > Build Solution**). Počkejte, než se během několika vteřin stáhnou potřebné balíčky.

5. Klikněte pravým tlačítkem na projekt a vyberte **Manage NuGet Packages...**

   ![1518003519163](/1518003519163.png)

6. Zvolte **Updates**, potom zaškrtněte **Select all packages** a klikněte na **Update**. 

7. Objeví-li se další dostupné aktualizace, zopakujte tento postup.

> Vetšinou je vhodné začínat vývoj s aktualizovanými balíčky.

Tím je příprava hotová. Když teď aplikaci spustíte klávesou **F5** (nebo tlačítkem se zelenou šipkou "Play"), otevře se okno prohlížeče. Přejděte do **Bot Framework Emulator**u, klikněte na pole s textem **Enter your endpoint URL** a vyberte stejný server, jako je v prohlížeči (v mém případě je to localhost:3979):

![1518003750998](/1518003750998.png)

Celá adresa pak bude `http://localhost:3979/api/messages`.

Ponechte prázdná pole **Microsoft App ID** a **Microsoft App Password** a klikněte na **Connect**.

Když teď botovi něco napíšete, odpoví vám:

![1518003927427](/1518003927427.png)

Tímto jsme ověřili, že vše funguje, a je čas zanořit se do kódu.

### MessagesController

Ve Visual Studiu zastavte ladění (**Shift + F5** nebo tlačítko "Stop"):

![1518004014080](/1518004014080.png)

V panelu Solution Explorer rozbalte složku Controllers a podívejte se na soubor **MessagesController.cs**.

Podstatná je metoda `Post()`, kam přijde každá zpráva od uživatele (aplikace ji nabízí na adrese `/api/messages`). Odtud pak putuje do dialogů - v našem případě je to `RootDialog`.

> Chatbot je vlastně webová aplikace, konkrétně [API](https://cs.wikipedia.org/wiki/API). V C# používáme technologii *ASP.NET WebAPI*. Alternativou může být JavaScript a *Node.js*.

### RootDialog poprvé

Ve složce Dialogs je soubor **RootDialog.cs**. Třída `RootDialog` implementuje rozhraní `IDialog<object>` a v metodě `MessageReceivedAsync()` zpracovává zprávu od uživatele. 

Důležitý je parametr `context` typu `IDialogContext`, který se předává mezi všemi operacemi v rámci dialogu a určuje, kam příchozí zpráva patří.

> Zpráva od uživatele není jenom text, ale také spousta informací okolo - jméno autora, účet, datum, konverzace a dialog, kam patří, stavové informace apod.

```c#
public Task StartAsync(IDialogContext context)
{
    ...
}

private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
{
    ...
}
```

Když upravíte text, který se posílá jako parametr metodě `PostAsync()`, změníte odpověď bota. Prosté...

```c#
await context.PostAsync($"You sent {activity.Text} which was {length} characters");
```

### Services

Tento bot bude ale "chytřejší" a využije externí službu. Na adrese http://yesno.wtf je k dispozici veřejné API, které vrací náhodně odpověď "yes" nebo "no" a k ní patřičný GIF. Náš bot, poradce, ji využije, aby dokázal odpovědět na libovolnou otázku.

![1518006751354](/1518006751354.png)

Nejprve si připravíme tzv. YesNoService:

1. V podokně **Solution Explorer** vytvořte v projektu novou složku. Pojmenujte ji **Services**.

   ![1518006863449](/1518006863449.png)

2. Klikněte na ni pravým tlačítkem a vyberte **Add > Class...**

3. Pojmenujte soubor **YesNoService.cs**.

4. Doplňte implementaci:

```c#
public class YesNoService
{
    public static async Task<YesNoModel> GetYesNoAsync(bool translate = false)
    {
        HttpResponseMessage res;

        using (HttpClient hc = new HttpClient())
        {
            res = await hc.GetAsync("https://yesno.wtf/api/");
        }

        if (res.IsSuccessStatusCode)
        {
            var yesNo = JsonConvert.DeserializeObject<YesNoModel>(await res.Content.ReadAsStringAsync());
            if (translate)
                yesNo.Answer = Translate(yesNo.Answer);

            return yesNo;
        }
        else
            return null;
    }

    public static string Translate(string yesNo)
    {
        string translation;

        switch (yesNo.ToLower())
        {
            case Answers.Yes:
                translation = "Ano";
                break;
            case Answers.No:
                translation = "Ne";
                break;
            default:
                translation = "Nejsem si jistý";
                break;
        }

        return translation;
    }
}
```

Co se tu odehrává?

* Pomocí *HttpClient* si vyžádáme odpověď od *YesNo API*.
* Přijatý řetězec ve formátu JSON převedeme pomocí knihovny *Json.NET* na C# objekt.
* Budeme-li chtít odpověď v češtině, přeložíme ji.
* Vrátíme hotovou odpověď k dalšímu zpracování (kdekoliv).

Kód je zatím plný červeně podtrhaných výrazů. Zatím si toho nebudeme všímat a začneme doplňovat, co mu chybí.

*YesNo API* vrací výsledek ve formátu JSON:

```json
{"answer":"yes","forced":false,"image":"https://yesno.wtf/assets/yes/6-304e564038051dab8a5aa43156cdc20d.gif"}
```

Připravíme si tedy odpovídající třídu v C#.

1. Přidejte do projektu novou složku, pojmenujte ji **Models**.
2. Přidejte do ní novou třídu (**Add > Class...**).
3. Pojmenujte soubor **YesNoModel.cs**.
4. Doplňte implementaci:

```c#
public class YesNoModel
{
    public string Answer { get; set; }
    public bool Forced { get; set; }
    public string Image { get; set; }
}
```

Abychom si udělali život v budoucnu jednodušší, přidáme ještě do stejného souboru konstanty pro jednotlivé typy odpovědí:

```c#
public static class Answers
{
    public const string Yes = "yes";
    public const string No = "no";
    public const string Maybe = "maybe";
}
```

Celý **YesNoModel.cs** pak bude vypadat takto:

```c#
namespace AnoNeBot.Models
{
    public class YesNoModel
    {
        public string Answer { get; set; }
        public bool Forced { get; set; }
        public string Image { get; set; }
    }

    public static class Answers
    {
        public const string Yes = "yes";
        public const string No = "no";
        public const string Maybe = "maybe";
    }
}
```

Vraťte se do **YesNoService.cs** a doplňte na začátek souboru:

```c#
using AnoNeBot.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
```

Všechna červená podtržení by měla zmizet.

### RootDialog podruhé

Nyní upravíme *RootDialog* tak, aby využíval nově připravenou službu. Co chceme, aby bot dělal?

* Přijal zprávu od uživatele.
* Zkontroloval, jestli se jedná o otázku.
* Vyžádal si od YesNoService odpověď Ano/Ne.
* Poslal tuto odpověď zpět uživateli.

Najděte v souboru **RootDialog.cs** metodu **MessageReceivedAsync()** a upravte ji následovně:

```c#
private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
{
    var activity = await result as Activity;

    if (!activity.Text.EndsWith("?"))
    {
        await context.PostAsync("To je zajímavé, ale dokud mi nepoložíš otázku, tak ti nemohu pomoci...");
    }
    else
    {
        var yesNo = await YesNoService.GetYesNoAsync(true);
        await context.PostAsync(yesNo.Answer);
    }
            
    context.Wait(MessageReceivedAsync);
}
```

`YesNoService` bude červěně podtržené. Přidejte proto na začátek souboru ještě:

```c#
using AnoNeBot.Services;
```

Spusťte aplikaci (**F5**), přejděte do **Bot Framework Emulatoru** a zkuste se bota na něco zeptat.

![1518009001857](/1518009001857.png)

### RootDialog s obrázky

Bot Framework umožňuje využít i grafické prvky dostupné chatbotům na různých kanálech. My využijeme tzv. HeroCard a kromě strohé jednoslovné odpovědi pošleme uživateli i animovaný GIF.

Upravte kód metody MessageReceivedAsync() tak, aby využívala kartu:

```c#
private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
{
    var activity = await result as Activity;

    if (!activity.Text.EndsWith("?"))
    {
        await context.PostAsync("To je zajímavé, ale dokud mi nepoložíš otázku, tak ti nemohu pomoci...");
    }
    else
    {
        var yesNo = await YesNoService.GetYesNoAsync(true);

        var reply = activity.CreateReply();
        var card = new HeroCard(yesNo.Answer)
        {
            Images = new List<CardImage>()
            {
                new CardImage(yesNo.Image)
            }
        };

        reply.Attachments.Add(card.ToAttachment());
        await context.PostAsync(reply);
    }
            
    context.Wait(MessageReceivedAsync);
}
```

A nahoru doplňte opět using:

```c#
using System.Collections.Generic;
```

Když aplikaci spustíte teď a položíte botovi otázku, dostanete mnohem bohatší odpověď.

![1518009627918](/1518009627918.png)

## Bot druhý - Kdo je to?

Ve druhém cvičení přidáme do chatbota nový dialog a ukážeme si práci se stavovými informacemi uživatele. Toto rozšíření pomůže se zapamatováním jmen nových lidí - bot nabídne fotku a uživatel bude muset uhodnout jméno člověka.

### Příprava

Budeme rozvíjet již vytvořený projekt, takže není potřeba zakládat nový.

1. Vytvořte v projektu novou složku, pojmenujte ji **Assets**.

2. Sežeňte fotky lidí, které se chcete naučit poznávat a vložte je do složky **Assets** (pravým tlačítkem ve Visual Studiu > **Add > Existing Item...**).

   ![1518100369875](/1518100369875.png)

3. Přidejte novou třídu do složky **Models**. Pojmenujte ji **PeopleModel** (Add > Class... > PeopleModel.cs).

4. Doplňte implementaci (nahraďte hodnoty vašimi názvy souborů a jmény, případně přidejte další řádky):

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

5. Přidejte using:

```c#
using System.Collections.Generic;
```

Tolik příprava zdrojových dat. Bot bude čerpat ze seznamu `People` a bude náhodně posílat obrázky a kontrolovat správnost jména (první a druhá hodnota). Celou tuto funkčnost zabalíme do nového *dialogu*.

### WhoIsDialog

Vytvořte ve složce **Dialogs** nový soubor typu **Bot Dialog** jménem **WhoIsDialog.cs** a vložte do něj implementaci metody `MessageReceivedAsync()`:

```c#
private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
{
    var activity = await result as IMessageActivity;

    if (context.ConversationData.ContainsKey("LastFace"))
    {
        var lastFace = context.ConversationData.GetValue<KeyValuePair<string, string>>("LastFace");
            
        if (activity.Text.ToLower() == lastFace.Value.ToLower())
        {
            await context.PostAsync("Správně!");
        }
        else
        {
            await context.PostAsync("Chyba! Je to " + lastFace.Value);
        }
    }

    await ShowRandomFaceAsync(context);

    context.Wait(MessageReceivedAsync);
}
```

A jako obvykle using:

```c#
using System.Collections.Generic;
```

Kdykoliv přijde zpráva od uživatele, tak se nejprve podíváme, zda už jsme se ho zeptali na jméno k obličeji. Pokud ano, vytáhneme si informace tohoto obličeje (tedy obrázek a jméno).

```c#
var lastFace = context.ConversationData.GetValue<KeyValuePair<string, string>>("LastFace");
```

A poté porovnáme, co nám přišlo ve zprávě, se jménem u daného obličeje.

```c#
if (activity.Text.ToLower() == lastFace.Value.ToLower())
```

> V našem případě je `lastFace.Value` jméno a `lastFace.Key` fotka.

Podle toho, jak dopadne vyhodnocení, pošleme uživateli patřičnou odpověď.

Na konci potom pošleme nový obličej a čekáme zase na odpověď.

Aby bot takto fungoval, zbývá ještě pod *MessageReceivedAsync()* doplnit metodu `ShowRandomFaceAsync()`.

### Show Random Face

V této pomocné metodě chceme ze seznamu obličejů náhodně jeden vybrat, sestavit grafickou kartu a tu poslat uživateli.

```c#
private async Task ShowRandomFaceAsync(IDialogContext context)
{
    Random rand = new Random();
    var face = PeopleModel.People.ElementAt(rand.Next(0, PeopleModel.People.Count));

    context.ConversationData.SetValue("LastFace", face);

    var root = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/";
    var card = new ThumbnailCard("Kdo je to?", images: new List<CardImage>() { new CardImage(root + face.Key) });

    var message = context.MakeMessage();
    message.Attachments.Add(card.ToAttachment());

    await context.PostAsync(message);
}
```

Usings na začátek souboru:

```c#
using AnoNeBot.Models;
using System.Linq;
using System.Web;
```

V přechozím cvičení jsme používali `HeroCard`, nyní zkoušíme `ThumbnailCard`, která má trochu jiné rozložení.

> Přehled všech typů karet najdete [v dokumentaci](https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-send-rich-cards).

V této metodě jsou klíčové dva momenty. Po vybrání náhodného obličeje si ho uložíme do `ConversationData`, protože až nám uživatel napíše odpověď, budeme chtít zkontrolovat, zda je správná (to už máme v kódu výše).

```c#
context.ConversationData.SetValue("LastFace", face);
```

Druhý důležitý moment je vygenerování "karty" s fotkou. Princip je stejný jako v předchozím cvičení, nicméně tady potřebujeme sestavit webovou adresu obrázku dynamicky, protože si jej hostujeme na webserveru sami.

```c#
var root = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/";
...
new CardImage(root + face.Key) // face.Key => "Assets/martin.jpg"
```

Výsledek potom pošleme uživateli.

Všimněte si, že se do metody předává `context`.

Celý WhoIsDialog by měl vypadat takto:

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
                await context.PostAsync("Správně!");
            }
            else
            {
                await context.PostAsync("Chyba! Je to " + lastFace.Value);
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
        var card = new ThumbnailCard("Kdo je to?", images: new List<CardImage>() { new CardImage(root + face.Key) });

        var message = context.MakeMessage();
        message.Attachments.Add(card.ToAttachment());

        await context.PostAsync(message);
    }
}
```

### Změna dialogu

Ještě před tím, že nový dialog vyzkoušíte, je potřeba změnit směrování zpráv v souboru **MessagesController.cs**, aby aplikace místo *RootDialogu* používala nový *WhoIsDialog*.

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

Pokud nyní aplikaci spustíte a napíšete chatbotovi zprávu, měli byste dostat obrázkovou odpověď:

![1518107478717](/1518107478717.png)

## Shrnutí

ABCD