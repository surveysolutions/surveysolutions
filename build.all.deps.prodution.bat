start cmd /c "TITLE Designer Questionnaire && cd src\UI\WB.UI.Designer && yarn && yarn gulp --production"
call cmd  /c "TITLE hq deps && cd src\UI\Headquarters\WB.UI.Headquarters\Dependencies && yarn && yarn gulp --production"
start cmd /c "TITLE Hq App && cd src\UI\Headquarters\WB.UI.Headquarters\HqApp && yarn && yarn gulp --production"

