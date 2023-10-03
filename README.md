Die to some unreconsilable differences in my and Microsoft's views on convinience, reasonable security risks, and potentioal force major factors, I had to move my repositories from from GitHub.

GitLab mirror for this repo: https://gitlab.com/krypt_lynx/ModDiff

# ModDiff
Alternatime Mods Mismatch window for RimWorld game

You need to modify search assembly paths to make this project to compile (check dependency-*.csproj files)
I probably need to create a configuration tool to generate those dependency files, but this yet to happen.

To compile and deploy the mod start `./Deploy/Deploy.bat`.
Deployment script requites:
- PowerShell 5.0 or above
- properly intalled 7zip
