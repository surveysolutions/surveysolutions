﻿using System.Configuration;

namespace WB.UI.Designer.Code.ConfigurationManager
{
    public class PdfConfigSection : ConfigurationSection
    {
        private const string INSTRUCTIONSEXCERPTLENGTH = "instructionsExcerptLength";
        private const string EXPRESSIONEXCERPTLENGTH = "expressionExcerptLength";
        private const string OPTIONSEXCERPTCOUNT = "optionsExcerptCount";
        private const string MINAMOUNTOFDIGITSINCODES = "minAmountOfDigitsInCodes";
        private const string ATTACHMENTSIZE = "attachmentSize";

        [ConfigurationProperty(INSTRUCTIONSEXCERPTLENGTH, IsRequired = true)]
        public PdfConfigurationElement InstructionsExcerptLength
        {
            get { return ((PdfConfigurationElement)(base[INSTRUCTIONSEXCERPTLENGTH])); }
            set { base[INSTRUCTIONSEXCERPTLENGTH] = value; }
        }

        [ConfigurationProperty(EXPRESSIONEXCERPTLENGTH, IsRequired = true)]
        public PdfConfigurationElement ExpressionExcerptLength
        {
            get { return ((PdfConfigurationElement)(base[EXPRESSIONEXCERPTLENGTH])); }
            set { base[EXPRESSIONEXCERPTLENGTH] = value; }
        }

        [ConfigurationProperty(OPTIONSEXCERPTCOUNT, IsRequired = true)]
        public PdfConfigurationElement OptionsExcerptCount
        {
            get { return ((PdfConfigurationElement)(base[OPTIONSEXCERPTCOUNT])); }
            set { base[OPTIONSEXCERPTCOUNT] = value; }
        }

        [ConfigurationProperty(MINAMOUNTOFDIGITSINCODES, IsRequired = true)]
        public PdfConfigurationElement MinAmountrOfDigitsInCodes
        {
            get { return ((PdfConfigurationElement)(base[MINAMOUNTOFDIGITSINCODES])); }
            set { base[MINAMOUNTOFDIGITSINCODES] = value; }
        }

        [ConfigurationProperty(ATTACHMENTSIZE, IsRequired = true)]
        public PdfConfigurationElement AttachmentSize
        {
            get { return ((PdfConfigurationElement)(base[ATTACHMENTSIZE])); }
            set { base[ATTACHMENTSIZE] = value; }
        }

        public class PdfConfigurationElement : ConfigurationElement
        {
            private const string VALUE = "value";

            [ConfigurationProperty(VALUE, IsRequired = true)]
            [IntegerValidator(MinValue = 0, MaxValue = 4096)]
            public int Value
            {
                get { return (int) base[VALUE]; }
                set { base[VALUE] = value; }
            }
        }
    }

}
