using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Capi.UI.MaskFormatter;

namespace WB.Core.BoundedContexts.Capi.Tests.UI.MaskedFormatterTests
{
    [Subject(typeof(MaskedFormatter))]
    internal class when_creating_mask_formatter_with_variety_of_symbols
    {
        Establish context = () =>
        {
        };

        Because of = () => {
                               maskedFormatter = new MaskedFormatterTestable(mask);
                               maskCharacters = maskedFormatter.MaskCharacters;
        };

        It should_first_mask_character_be_digital = () =>
            maskCharacters[0].ShouldBeOfExactType<DigitMaskCharacter>();

        It should_second_mask_character_be_literal = () =>
            maskCharacters[1].ShouldBeOfExactType<LiteralCharacter>();

        It should_third_mask_character_be_upper_case_character = () =>
            maskCharacters[2].ShouldBeOfExactType<CharCharacter>();

        It should_fourth_mask_character_be_lower_case_character = () =>
            maskCharacters[3].ShouldBeOfExactType<MaskCharacter>();

        It should_fifth_mask_character_be_alpha_numeric_character = () =>
            maskCharacters[4].ShouldBeOfExactType<LiteralCharacter>();

        private static MaskedFormatterTestable maskedFormatter;
        private static MaskCharacter[] maskCharacters;
        private static string mask = @"#-~*\'";
    }

    internal class MaskedFormatterTestable : MaskedFormatter
    {
        public MaskedFormatterTestable(string mask, string validCharacters = null, string invalidCharacters = null, string placeholder = null, char placeholderCharacter = '_')
            : base(mask, validCharacters, invalidCharacters, placeholder, placeholderCharacter) {}
        
        public MaskCharacter[] MaskCharacters { get { return this.maskChars; } }
    }
}
