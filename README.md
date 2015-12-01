- Added new section for event bus to HQ/SV/D web configs. Make sure that all updated to 5.3.0 clients have it.
  <section name="eventBus" type="WB.UI.Shared.Web.Configuration.EventBusConfigSection, WB.UI.Shared.Web, Version=3.0.60.0, Culture=neutral" />
  <eventBus>
    <eventHandlers>
      <disabled>
        <!--<add type="WB.Core.SharedKernels.SurveyManagement.EventHandler.InterviewHistoryDenormalizer, WB.Core.SharedKernels.SurveyManagement"/>-->
      </disabled>
      <withIgnoredExceptions>
        <add type="WB.Core.SharedKernels.SurveyManagement.EventHandler.InterviewStatusTimeSpanDenormalizer, WB.Core.SharedKernels.SurveyManagement"/>
        <add type="WB.Core.SharedKernels.SurveyManagement.EventHandler.InterviewsChartDenormalizer, WB.Core.SharedKernels.SurveyManagement"/>
        <add type="WB.Core.SharedKernels.SurveyManagement.EventHandler.AnswersByVariableDenormalizer, WB.Core.SharedKernels.SurveyManagement"/>
      </withIgnoredExceptions>
    </eventHandlers>
  </eventBus>
- Rebuild read side on SV/HQ/D is needed for 5.3.0 version (added functionality for check readside version, first time rebuild readside is requered)