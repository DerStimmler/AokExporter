# AOK Exporter

Exporter for data from the statutory health insurance company AOK

## Usage

### Export patient receipts (Patientenquittungen)

1. [Log in](https://meine.aok.de/) to your AOK Account
2. Execute the follwing code into the Dev-Tools of your browser to retrieve your access and refresh tokens from localStorage and create the required program arguments with them.
```js
console.log(`-a ${JSON.parse(localStorage.getItem("spartacus⚿⚿auth")).token.access_token} -r ${JSON.parse(localStorage.getItem("spartacus⚿⚿auth")).token.refresh_token}`)
```
3. Execute the program with these arguments
```powershell
./AokExporter.exe -a ey.... -r ory_rt_...
```
4. Your export is available in `./export`

## Options

| Option                | Description            | Required |
|-----------------------|------------------------|----------|
| `-a` `--accessToken`  | Sets the access token  | ✔        |
| `-r` `--refreshToken` | Sets the refresh token | ✔        |


## Hints

- You're access token is only valid for 5 min. so you have to be faster than that when copying your token and starting the program.
- Don't navigate the website after copying the tokens because it's possible that a new refresh token gets generated and the old one gets stale.