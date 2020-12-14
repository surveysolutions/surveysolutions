using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using StatData.Writers.Stata;

namespace StatData.Writers
{
    /// <summary>
    /// Value set representation for Stata (all versions)
    /// </summary>
    internal class StataXValueSet
    {
        private Encoding _encoding;
        
        internal StataXValueSet(Encoding enc) // CTOR
        {
            _encoding = enc;
            _lbls = new List<StataSingleValueLabel>();
        }

        internal StataXValueSet(IDataAccessorSimple data, int v, Encoding enc) // CTOR
        {
            _encoding = enc;
            _lbls = new List<StataSingleValueLabel>();

            // populate with values from data accessor
            var vN = data.GetDctSize(v);
            if (vN <= 0) return;

            Name = data.GetVarName(v);

            for (var i = 0; i < vN; i++)
            {
                try
                {
                    Lbls.Add(
                        new StataSingleValueLabel
                        {
                            Text = data.GetDctLabel(v, i),
                            Value = data.GetDctCode(v, i)
                        });
                }
                catch (OverflowException e)
                {
                    // if an overflow occured, we ignore this label
                }
            }
        }

        internal string Name { get; set; }

        private readonly List<StataSingleValueLabel> _lbls;
        internal List<StataSingleValueLabel> Lbls
        {
            get { return _lbls; }
        }

        private Int32[] Data { get; set; }       // calculated
        private byte[] Txt { get; set; }         // calculated

        private byte[] ValueLabelTable
        {
            get
            {
                var result = new byte[Data.Length * 4 + Txt.Length];

                var p = 0;
                for (var i = 0; i < Data.Length; i++)
                {
                    var z = BitConverter.GetBytes(Data[i]);
                    result[p] = z[0];
                    result[p + 1] = z[1];
                    result[p + 2] = z[2];
                    result[p + 3] = z[3];
                    p = p + 4;
                }

                for (var i = 0; i < Txt.Length; i++)
                {
                    result[p] = Txt[i];
                    p = p + 1;
                }

                return result;
            }
        }

        internal void Construct()
        {
            Lbls.Sort();

            var n = Lbls.Count;

            var l = 0;
            for (var i = 0; i < n; i++)
                l = l + _encoding.GetBytes(Lbls[i].Text).Length + 1;

            Data = new Int32[2 + 2 * n];

            Data[0] = n;

            Txt = new byte[l];

            var p = 0;
            for (var i = 0; i < n; i++)
            {
                Data[i + 2] = p;
                Data[i + 2 + n] = Lbls[i].Value;
                var li = Lbls[i].Text;
                var liBytes = _encoding.GetBytes(li);
                Array.Copy(liBytes, 0, Txt, p, liBytes.Length);
                p = p + liBytes.Length;
                Txt[p] = 0;
                p++;
            }
            Data[1] = p;
        }

        internal byte[] ToBytes(int vnWidth) // length*1 or 4
        {
            using (var ms = new MemoryStream())
            {
                using (var w = new BinaryWriter(ms))
                {
                    var vt = ValueLabelTable;
                    w.Write((Int32)vt.Length);
                    var nameBytes = _encoding.GetBytes(Name); // assuming this is a conforming NAME
                    w.Write(nameBytes);

                    if (nameBytes.Length < vnWidth)
                        w.Write(new byte[vnWidth - nameBytes.Length]); // padding
                    w.Write((Int32)0); // for 0 terminator and 3 zeroes padding
                    w.Write(vt);
                    w.Close();

                    return ms.ToArray();
                }
            }
        }
    }
}

