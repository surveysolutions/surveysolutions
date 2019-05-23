<template>
  <HqLayout :fixedWidth="false">
    <div slot="headers">
      <h1>
        {{$t('DataExport.DataExport_Title')}}
      </h1>
      <div class="row">
        <div class="col-lg-5">
          <p>Data export set will be generated based on selected version of questionaire, timeline and statuses and can contain additional data (DDI structure, paradata, collected binary data)</p>
        </div>
      </div>
    </div>
    <div class="col-md-12">
     <div class="row" v-if="exportServiceIsUnavailable">
        <div class="col-md-12 mb-30">
          Export service is not available
        </div>
    </div>
    
    <div class="row">
      <div class="export d-flex">
        <div class="col-md-12">
          <form v-if="!exportServiceIsUnavailable">
            <div class="mb-30">
                <h3>Filter interviews to export</h3>
                <div class="d-flex mb-20 filter-wrapper">
                  <div class="filter-column">
                    <h5>Survey template (questionnaire version)</h5>
                    <div class="form-group">
                      <Typeahead
                        control-id="questionnaireId"
                        fuzzy
                        :placeholder="$t('Common.AllQuestionnaires')"
                        data-vv-name="questionnaireId"
                        data-vv-as="questionnaire"
                        v-validate="'required'" 
                        :value="questionnaireId"
                        v-on:selected="questionnaireSelected"
                        :fetch-url="questionnaireFetchUrl"
                      />
                    </div>
                    <div class="form-group">
                      <Typeahead
                        control-id="questionnaireVersion"
                        ref="questionnaireVersionControl"
                        data-vv-name="questionnaireVersion"
                        data-vv-as="questionnaireVersion"
                        v-validate="'required'"
                        :placeholder="$t('Common.AllVersions')"
                        :value="questionnaireVersion"
                        :fetch-url="questionnaireVersionFetchUrl"
                        v-on:selected="questionnaireVersionSelected"
                        :disabled="questionnaireVersionFetchUrl == null"
                        :selectFirst=true
                      />
                  </div>
                    </div>
                    
                  
                  <div class="filter-column">
                    <h5>Status of exported interviews</h5>
                    <Typeahead
                      control-id="status"
                      fuzzy
                      data-vv-name="status"
                      data-vv-as="status"
                      :placeholder="$t('Common.AllStatuses')"
                      :value="status"
                      :values="statuses"
                      v-on:selected="statusSelected"
                    />
                  </div>
                 </div>
            </div>
            <div class="mb-30">
                <h3>Data type</h3>
                <div class="radio-btn-row" v-if="hasInterviews">
                  <input class="radio-row" type="radio" name="dataType" id="surveyData" v-model="dataType" value="surveyData">
                  <label for="surveyData" class="" >
                    <span class="tick"></span>
                    <span class="format-data Binary">{{$t('DataExport.MainSurveyDataTitle')}}. {{$t('DataExport.ZipArchiveDescription')}}</span>
                  </label>
                </div>
                <div class="radio-btn-row" v-if="hasInterviews && hasBinaryData">
                  <input class="radio-row" type="radio" name="dataType" id="binaryData"  v-model="dataType" value="binaryData">
                  <label for="binaryData">
                    <span class="tick"></span>
                    <span class="format-data Binary">Binary Data. Archive with binary data (e.g., pictures, audio)</span>
                  </label>
                </div>
                <div class="radio-btn-row" v-if="questionnaireVersion">
                  <input class="radio-row" type="radio" name="dataType" id="ddiData" v-model="dataType" value="ddiData">
                  <label for="ddiData">
                    <span class="tick"></span>
                    <span class="format-data">DDI. Data Documentation Initiative XML data</span>
                  </label>
                </div>
                <div class="radio-btn-row" v-if="hasInterviews">
                  <input class="radio-row" type="radio" name="dataType" id="paraData" v-model="dataType" value="paraData">
                  <label for="paraData">
                    <span class="tick"></span>
                    <span class="format-data Tabular">Paradata. Metadata on the interview process (events and timing)</span>
                  </label>
                </div>
            </div>
            <div class="mb-30" v-if="dataType == 'surveyData'">
                <h3>Data format</h3>
                <div class="radio-btn-row">
                  <input class="radio-row" type="radio" name="dataFormat" id="separated" v-model="dataFormat" value="Tabular">
                  <label for="separated">
                    <span class="tick"></span>
                    <span class="format-data Tabular">Tab separated data</span>
                  </label>
                </div>
                <div class="radio-btn-row">
                  <input class="radio-row" type="radio" name="dataFormat" id="Stata"  v-model="dataFormat" value="Stata">
                  <label for="Stata" class="">
                    <span class="tick"></span>
                    <span class="format-data STATA">Stata 10 format (no Unicode)</span>
                  </label>
                </div>
                <div class="radio-btn-row">
                  <input class="radio-row" type="radio" name="dataFormat" id="Spss" v-model="dataFormat" value="Spss">
                  <label for="Spss">
                    <span class="tick"></span>
                    <span class="format-data SPSS">SPSS format</span>
                  </label>
                </div>
            </div>
            <div class="mb-30" v-if="canExportExternally">
                <h3>Export file destination</h3>
                <div class="radio-btn-row">
                  <input class="radio-row" type="radio" name="exportDestination" id="download"  v-model="dataDestination" value="zip">
                  <label for="download">
                    <span class="tick"></span>
                    <span class="export-destination">Download</span>
                  </label>
                </div>
                <div class="radio-btn-row" v-if="isDropboxSetUp">
                  <input class="radio-row" type="radio" name="exportDestination" id="onedrive"  v-model="dataDestination" value="oneDrive">
                  <label for="onedrive">
                    <span class="tick"></span>
                    <span class="export-destination OneDrive">Upload to OneDrive</span>
                  </label>
                </div>
                <div class="radio-btn-row" v-if="isDropboxSetUp">
                  <input class="radio-row" type="radio" name="exportDestination" id="dropbox"  v-model="dataDestination" value="dropbox">
                  <label for="dropbox">
                    <span class="tick"></span>
                    <span class="export-destination Dropbox">Upload to Dropbox</span>
                  </label>
                </div>
                <div class="radio-btn-row" v-if="isGoogleDriveSetUp">
                  <input class="radio-row" type="radio" name="exportDestination" id="googleDrive" v-model="dataDestination" value="googleDrive">
                  <label for="googleDrive">
                    <span class="tick"></span>
                    <span class="export-destination GoogleDrive">Upload to Google Drive</span>
                  </label>
                </div>
            </div>
            <div class="mb-30">
                <div class="structure-block">
                    <button type="button" class="btn btn-success" @click="queueExport">add to queue</button>
                    <button type="button" class="btn btn-lg btn-link" @click="exportFormIsVisible = false">Cancel</button>
                </div>
            </div>
          </form>
        </div>
        <div class="col-md-12" v-if="!exportServiceIsUnavailable">
          <div class="no-sets" v-if="!exportServiceIsUnavailable && exportResults.length == 0">
            <p>No generated sets yet</p>
            <button  v-if="!exportFormIsVisible"  type="button" class="btn btn-success" @click="exportFormIsVisible = true">Generate new export set</button>
          </div>
          <div v-else>
            <h3 class="mb-20">Previously generated export sets</h3>
            <p>Every set is a zip archive with all collected interview data and DDI XML structure you an download previously generated reports</p>
            <ExportProcessCard  v-for="result in exportResults" v-bind:key="result.id" :data="result"></ExportProcessCard>
          </div>
        </div>
      </div>
    </div>
    <div class="row" v-if="!exportServiceIsUnavailable">
      <div class="col-lg-5">
          <h3 class="mb-20">Data export API</h3>
          <p>You can setup automatic export of collected interview data using our API toolset for more information 
              and inctruction use <a href="#" target="_blank" class="underlined-link">Data Export API reference</a>
          </p>
      </div>
    </div>
    </div>
  </HqLayout>
</template>

<script>
import Vue from "vue"
import ExportProcessCard from "./ExportProcessCard"
import {mixin as VueTimers} from 'vue-timers'

 const dataFormatNum = {  
  Tabular: 1,
  Stata: 2,
  Spss: 3,
  Binary: 4,
  Ddi: 5,
  Paradata: 6
};
const ExternalStorageType =
{
    dropbox: 1,
    oneDrive: 2,
    googleDrive: 3
};

export default {
    mixins: [VueTimers],
    data() {
        return {
            exportServiceIsUnavailable: true,
            exportFormIsVisible: false,
            dataType: null,
            dataFormat: "Tabular",
            dataDestination: "zip",
            questionnaireId: null,
            questionnaireVersion: null,
            status: null,
            statuses: this.$config.model.statuses,
            exportResults: [],
            isUpdatingDataAvailability: false,
            hasInterviews: false,
            hasBinaryData: false,
            externalStoragesSettings: (this.$config.model.externalStoragesSettings || {}).oAuth2 || {}
        };
    },
    timers: {
      updateExportCards: { time: 5000, autostart: true, repeat: true }
    },
    created() {
        var self = this;
        self.$store.dispatch("showProgress");

        this.$http.get(this.$config.model.api.statusUrl)
            .then(function (response) {

              if (response.data)
                self.exportServiceIsUnavailable = false;

              let exportIds = response.data || [];
              for(let i = 0;i< exportIds.length;i++)
              {
                self.exportResults.push({ 
                  id: exportIds[i]
                });
              }
            })
            .catch(function (error) {
                Vue.config.errorHandler(error, self);
            })
            .then(function () {
                self.$store.dispatch("hideProgress");
            });
    },
    computed: {
        isDropboxSetUp()
        {
          var settings = this.externalStoragesSettings["dropbox"] || null;
          return settings!=null;
        },
        isOneDriveSetUp()
        {
          var settings = this.externalStoragesSettings["oneDrive"] || null;
          return settings!=null;
        },
        isGoogleDriveSetUp()
        {
          var settings = this.externalStoragesSettings["googleDrive"] || null;
          return settings!=null;
        },
        canExportExternally(){
            return this.$config.model.externalStoragesSettings != null && (this.dataType == 'surveyData' || this.dataType == 'binaryData'); 
        },
        questionnaireFetchUrl() {
            return this.$config.model.api.questionnairesUrl;
        },
        questionnaireVersionFetchUrl() {
            if (this.questionnaireId && this.questionnaireId.key)
                return `${this.$config.model.api.questionnairesUrl}/${this.questionnaireId.key}`;
            return null;
        }
    },
    watch: {},
    methods: {
      updateExportCards(){
        var self = this;
        this.$http.get(this.$config.model.api.statusUrl)
            .then((response) => {

              self.exportServiceIsUnavailable = response.data == null;

              let exportIds = response.data || [];
              if (exportIds.length > 0)
              {
                var existingJobIndex = 0;
                var incomingJobIndex = 0;
                while(incomingJobIndex < exportIds.length && existingJobIndex < self.exportResults.length)
                {
                  var existingProcessId = self.exportResults[existingJobIndex].id;
                  var newProcessId = exportIds[incomingJobIndex];
                  if (existingProcessId == newProcessId)
                  {
                    existingJobIndex++;
                    incomingJobIndex++;
                  }
                  else if (existingProcessId < newProcessId)
                  {
                    self.exportResults.splice(existingJobIndex, 0, { id: newProcessId})
                    existingJobIndex++;
                    incomingJobIndex++;
                  }
                  else{
                    existingJobIndex++;
                  }
                }
                for(let i=incomingJobIndex;i<exportIds.length;i++)
                {
                  self.exportResults.push({ id: exportIds[i]});
                }
              }
            })
            .catch(function (error) {
                Vue.config.errorHandler(error, self);
            })
            .then(function () {
                self.$store.dispatch("hideProgress");
            });
      },
      async queueExport(){

        if (this.dataDestination != "zip")
        {
          this.redirectToExternalStorage();
          return;
        }

        var self = this;
        var validationResult = await this.$validator.validateAll();
        if (validationResult)
        {
            const exportParams = self.getExportParams(
              self.questionnaireId.key,
              self.questionnaireVersion.key,
              self.dataType,
              self.dataFormat,
              self.dataDestination,
              (self.status || { value: null}).value
            );

            self.$store.dispatch("showProgress");

            this.$http.post(this.$config.model.api.updateSurveyDataUrl, null, { params: exportParams })
                .then(function (response) {
                    self.$validator.reset();
                    self.exportFormIsVisible = false;
                    const jobId = (response.data || { JobId: 0 }).JobId;
                    self.exportResults.splice(0, 0,{ 
                      id: jobId
                    });
                })
                .catch(function (error) {
                    Vue.config.errorHandler(error, self);
                })
                .then(function () {
                    self.$store.dispatch("hideProgress");
                });
        }else{
            var fieldName = this.errors.items[0].field;
            const $firstFieldWithError = $("#"+fieldName);
            $firstFieldWithError.focus();
        }        
      },
      redirectToExternalStorage(){

        const exportParams = this.getExportParams(
            this.questionnaireId.key,
            this.questionnaireVersion.key,
            this.dataType,
            this.dataFormat,
            this.dataDestination,
            (this.status || { value: null}).value
          );

        var state = {
          questionnaireIdentity: {
              questionnaireId: exportParams.id,
              version: exportParams.version,
          },
          format: exportParams.format,
          interviewStatus: exportParams.status,
          type: ExternalStorageType[this.dataDestination]
        };

        let storageSettings = this.externalStoragesSettings[this.dataDestination];
        var request = {
          response_type: this.externalStoragesSettings.responseType,
          redirect_uri: encodeURIComponent(this.externalStoragesSettings.redirectUri),
          client_id: storageSettings.clientId,
          state: window.btoa(window.location.href + ";" + this.$config.model.api.exportToExternalStorageUrl + ";" + JSON.stringify(state)),
          scope: storageSettings.scope
        };

        window.location = storageSettings.authorizationUri + "?" + decodeURIComponent($.param(request));
      },
      getExportParams(questionnaireId, questionnaireVersion, dataType, dataFormat, dataDestination, status){
       
        var result = dataFormatNum.Tabular;
        switch(dataType)
        {
          case "surveyData": result = dataFormatNum[dataFormat]; break;
          case "binaryData": result = dataFormatNum.Binary; break;
          case "ddiData": result = dataFormatNum.Ddi; break;
          case "paraData":  result = dataFormatNum.Paradata; break;
        }

        return {
          id: questionnaireId,
          version: questionnaireVersion,
          format: dataFormat, 
          status: status
        };
      },
        statusSelected(newValue) {
            this.status = newValue;
        },
        questionnaireSelected(newValue) {
            this.questionnaireId = newValue;
        },
        questionnaireVersionSelected(newValue) {
            this.questionnaireVersion = newValue;
            if (this.questionnaireVersion)
              this.updateDataAvalability();
        },
        updateDataAvalability(){
          this.isUpdatingDataAvailability = true;
          
          this.$http.get(this.$config.model.api.dataAvailabilityUrl, { params: {
            id: this.questionnaireId.key,
            version: this.questionnaireVersion.key
          } })
            .then((response) => {
              this.hasInterviews = response.data.hasInterviews;
              this.hasBinaryData = response.data.hasBinaryData;
              this.dataType = this.hasInterviews ? "surveyData" : "ddi";
            })
            .catch((error) => {
                Vue.config.errorHandler(error, self);
            })
            .then(() => {
                this.isUpdatingDataAvailability = false;
            });
        }
    },
    components: {ExportProcessCard}
};
</script>
