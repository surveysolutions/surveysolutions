  5.8
- Run utlity EventStoreToPlainStorageMigrator to copy users and questionnaires to the plain storage if you upgrade version lower then 5.7. 
  The tool uses data from eventstore only, so the state of current readside doesn't really matter. 
  Example: EventStoreToPlainStorageMigrator -c "both" -p "Server = 127.0.0.1; Port = 5432; User Id = postgres; Password = Qwerty1234; Database = SuperHQ-Plain" -i "127.0.0.1" -t 1113 -h 2113 -l admin -s changeit

- Run utlity EventStoreToPlainStorageMigrator to copy users to the plain storage if you upgrade version 5.7. 
  The tool uses data from eventstore only, so the state of current readside doesn't really matter. 
  Example: EventStoreToPlainStorageMigrator -c "u" -p "Server = 127.0.0.1; Port = 5432; User Id = postgres; Password = Qwerty1234; Database = SuperHQ-Plain" -i "127.0.0.1" -t 1113 -h 2113 -l admin -s changeit
  

