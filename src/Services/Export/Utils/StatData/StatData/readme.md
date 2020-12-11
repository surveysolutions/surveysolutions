StatData is a library to facilitate output to native binary formats of popular statistical packages.

Writing is performed by a dataset writers implementing StatData.Writers.IDatasetWriter interface.

Writing can be performed to a file or to a stream. It is the user's responsibility to open and close the stream.

DatasetWriters can be created directly, by calling their respective constructors.

Writers don't have memory and can be reused for multiple datasets.

Data to be saved should be arranged in a string matrix or provided through a StatData.Core.IDataQuery interface that DatasetWriters receive as input. Optionally meta information may be supplied in an object of it's own class. Meta information includes at least a list of variables' names, but optionally may contain dataset label and timestamp, variable labels, and value labels.

Each value labels set is applied to its own variable.

Variable names are validated at time of WRITING!

Dataset for writing may not be empty (must contain at least one row and at least one column).
