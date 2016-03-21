 5.7
- !!!Do this BEFORE REBUILD READSIDE!!! Run utlity QToPlainStore to copy questionnaires to the plain storage. 
  Read side should be valid and not empty. 
  Example: QToPlainStore -p "Server=127.0.0.1;Port=5432;User Id=postgres;Password=P@$$w0rd;Database=SuperHQ-Plain" -i "127.0.0.1" -t 1113 -h 2113 -l admin -s changeit