/**

Description of the library

The library is intended to produce DDI description files from .Net applications.
DDI or Data Documentation Initiative is an effort to develop an international 
standard for describing data from social, behavioral, and economic sciences. More 
information about the DDI standard can be found at: 
<a href="http://www.ddialliance.org">http://www.ddialliance.org</a>
 
The developer needs to create an object of class MetaDescription, then set its 
properties Document and Study, add descriptions of data files (if available) 
and their variables. Once the meta information is set, call the WriteXml method
to write the resulting DDI file to athe specified file or a stream.


