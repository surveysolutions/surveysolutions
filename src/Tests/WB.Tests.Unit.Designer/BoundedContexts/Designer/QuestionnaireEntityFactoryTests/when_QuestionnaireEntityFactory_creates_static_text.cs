﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireEntityFactoryTests
{
    internal class when_QuestionnaireEntityFactory_creates_static_text : QuestionnaireEntityFactoryTestContext
    {
        Establish context = () =>
        {
            factory = CreateFactory();
        };

        Because of = () =>
            resultEntity = factory.CreateStaticText(entityId: entityId, text: text, attachmentName: attachmentName);

        It should_create_text_question = () =>
            resultEntity.ShouldBeOfExactType<StaticText>();

        It should_create_static_text_with_EntityId_equals__to_specified_entity_id__ = () =>
            resultEntity.PublicKey.ShouldEqual(entityId);

        It should_create_static_text_with_Text_equals__to_specified_text__ = () =>
            resultEntity.Text.ShouldEqual(text);

        It should_create_static_text_with_specified_AttachmentName = () =>
            resultEntity.AttachmentName.ShouldEqual(attachmentName);

        private static QuestionnaireEntityFactory factory;
        private static IStaticText resultEntity;

        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static string text = "some text";
        private static string attachmentName = "bananas";
    }
}