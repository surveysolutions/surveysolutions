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