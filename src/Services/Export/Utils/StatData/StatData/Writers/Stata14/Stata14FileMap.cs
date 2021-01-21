namespace StatData.Writers.Stata14
{
    internal class Stata14FileMap
    {
        internal ulong OffStataData;
        internal ulong OffMap;
        internal ulong OffVariableTypes;
        internal ulong OffVarnames;
        internal ulong OffSortlist;
        internal ulong OffFormats;
        internal ulong OffValueLabelNames;
        internal ulong OffVariableLabels;
        internal ulong OffCharacteristics;
        internal ulong OffData;
        internal ulong OffStrls;
        internal ulong OffValueLabels;
        internal ulong OffStataDataEnd;
        internal ulong OffEndOfFile;

        internal void WriteToStream(Core.StreamWriterSpecial fw) {
            fw.WriteStr("<map>");
            fw.Write(OffStataData);
            fw.Write(OffMap);
            fw.Write(OffVariableTypes);
            fw.Write(OffVarnames);
            fw.Write(OffSortlist);
            fw.Write(OffFormats);
            fw.Write(OffValueLabelNames);
            fw.Write(OffVariableLabels);
            fw.Write(OffCharacteristics);
            fw.Write(OffData);
            fw.Write(OffStrls);
            fw.Write(OffValueLabels);
            fw.Write(OffStataDataEnd);
            fw.Write(OffEndOfFile);
            fw.WriteStr("</map>");
        }
    }
}
