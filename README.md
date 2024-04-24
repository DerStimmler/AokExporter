# AOK Exporter

Exporter for data from the statutory health insurance company AOK

## Usage

### Export patient receipts (Patientenquittungen)

1. [Log in](https://meine.aok.de/) to your AOK Account
2. Execute the following code in your browsers DevTools to retrieve your access and refresh tokens from localStorage and create the required program options
```js
console.log(`-a ${JSON.parse(localStorage.getItem("spartacus⚿⚿auth")).token.access_token} -r ${JSON.parse(localStorage.getItem("spartacus⚿⚿auth")).token.refresh_token}`)
```
3. Execute the program with these options
```powershell
./AokExporter.exe -a ey.... -r ory_rt_...
```
4. Your export is available in your output directory (see option `--outputDirectory`). It follows this directory structure:
```text
outputDirectory/
├─ serviceGroup1/
│  ├─ serviceGroup1.json
│  ├─ case1/
│  │  ├─ case1.json
│  ├─ case2/
│  │  ├─ case2.json
│  ├─ case3/
│  │  ├─ case3.json
├─ serviceGroup2/
│  ├─ serviceGroup2.json
│  ├─ case4/
│  │  ├─ case4.json
│  ├─ case5/
│  │  ├─ case5.json
├─ serviceGroup3/
│  ├─ serviceGroup3.json
│  ├─ case6/
│  │  ├─ case6.json
```
E.g.:
```text
outputDirectory/
├─ Ärztliche Leistungen/
│  ├─ Ärztliche Leistungen.json
│  ├─ Dr. med. Erika Mustermann/
│  │  ├─ case1.json
│  ├─ Dr. med. Max Mustermann/
│  │  ├─ case2.json
├─ Zahnärztliche Leistungen/
│  ├─ Zahnärztliche Leistungen.json
│  ├─ Dr. med. Max Meier/
│  │  ├─ case4.json
│  ├─ Dr. Hermann Maxe/
│  │  ├─ case5.json
├─ Hilfsmittel/
│  ├─ Hilfsmittel.json
│  ├─ Apotheke GmbH/
│  │  ├─ case6.json
```

## Options

| Option                   | Description                                                          | Required                      |
|--------------------------|----------------------------------------------------------------------|-------------------------------|
| `-a` `--accessToken`     | Sets the access token                                                | ✔                             |
| `-r` `--refreshToken`    | Sets the refresh token                                               | ✔                             |
| `-f` `--from`            | Sets the date from which data is collected (_Format:_ `yyyy-MM-dd`)  | _Default:_ min available date |
| `-t` `--to`              | Sets the date until which data is collected (_Format:_ `yyyy-MM-dd`) | _Default:_ max available date |
| `-o` `--outputDirectory` | Sets the output directory                                            | _Default:_ `./export`         |


## Hints

- You're access token is only valid for 5 min. so you have to be faster than that when copying your token and starting the program.
- Don't navigate the website after copying the tokens because it's possible that a new refresh token gets generated and the old one gets stale.
- Should your tokens not work refresh the page and retrieve the tokens again.