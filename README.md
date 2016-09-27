5.14
- Compatible with EventStore 3.9
5.13 
- When updating Designer should be executed 'move-some-designer-rs-tables-to-plain.sql' from the same folder as README.md. Should be executed on plain database. Before execution should be changed connection string to read side database in 'move-some-designer-rs-tables-to-plain.sql' file in line:
PERFORM dblink_connect('views','host=127.0.0.1 port=5432 dbname=Design-Views user=postgres password=P@$$w0rd');
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