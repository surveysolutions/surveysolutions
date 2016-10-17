5.14
- Designer update: 
-- execute 'move-account-designer-readside-tables-to-plain.sql' on plain storage db with actual (updated) connection string at 'PERFORM dblink_connect'
-- execute 'migrator_3_to_1.ps1' with database name: "migrator_3_to_1.ps1 Design" or "migrator_3_to_1.ps1 SuperHQ"
   this script avtomaticaly will find databases "*-Plain", "*-Views" and "*-Evn" and move to one db with 3 scheams
   Example: if we execute "migrator_3_to_1.ps1 Design" script will try find "Design-Plain", "Design-Views" and "Design-Evn" and then create one db "Design" with 3 schemas
- Compatible with EventStore 3.9
- .NET Framework 4.6.2 is required to run HQ/D web sites https://www.microsoft.com/en-us/download/details.aspx?id=53344
5.13 
- When updating Designer should be executed 'move-some-designer-rs-tables-to-plain.sql' from the same folder as README.md. Should be executed on plain database. Before execution should be changed connection string to read side database in 'move-some-designer-rs-tables-to-plain.sql' file in line:
PERFORM dblink_connect('views','host=127.0.0.1 port=5432 dbname=Design-Views user=postgres password=Qwerty1234');
5.11
- Changed Designer endpoint for Headquartes. Should be updated DesignerAddress app settings parameter in all hearduarters build stripts. 
  New urls should be:
-- For prodaction: https://solutions.worldbank.org
-- For rc: https://design-rc.mysurvey.solutions
-- For dev: https://design-dev.mysurvey.solutions 
-- For local dev: http://localhost/designer
5.10
- Compatible with EventStore 3.6.2
- When upgading any previous version before 5.10 dbup tool needs to be executed. Following parameters should be used:
- when upgrading HQ app:
  dbup hq-up-plain /cs:"ConnectionString" 
  dbup hq-up-rs /cs:"ConnectionString"
Both commands a needed to update plain storage and read side.
-- when upgrading Designer app following commands should be executed:
  dbup des-up-plain /cs:"ConnectionString" 
  dbup des-up-rs /cs:"ConnectionString"
Tool can be found using this URL: https://wbg.box.com/s/xsvo4yb27j0q4pykky44jb73dvyamuz2