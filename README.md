 5.7
- Run utlity QToPlainStore to copy questionnaires to the plain storage. 
  The tool uses data from eventstore only, so the state of current readside doesn't really matter. 
  Example: QToPlainStore -p "Server=127.0.0.1;Port=5432;User Id=postgres;Password=P@$$w0rd;Database=SuperHQ-Plain" -i "127.0.0.1" -t 1113 -h 2113 -l admin -s changeit