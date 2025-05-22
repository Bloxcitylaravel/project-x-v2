@echo STARTING SITE
start cmd /k cd "2016-roblox-main" ^& call start.bat
start cmd /k cd "AssetValidationServiceV2" ^& call run.bat
start cmd /k cd "Roblox" ^& call dev.bat
timeout /t 20 /nobreak
@echo STARTING RCC
start cmd /k cd "game-server" ^& call start.bat
start cmd /k cd "RCCService" ^& call run.bat