<template>
    <HqLayout :hasFilter="false" :title="$t('Pages.EmailProvidersTitle')">
        <div class="row contain-input">
            
            <div class="col-sm-7 ">
            <div v-if="loading">Loading</div>
            <form else data-vv-scope="settings">
                <div class="radio">
                    <div class="field">
                        <input v-validate="'required'" name="provider" class="wb-radio" type="radio" v-model="provider" id="provider_none"  value="none">
                        <label for="provider_none"><span class="tick"></span>{{ $t('Settings.EmailProvider_None') }}</label>
                    </div>
                </div>
                <p>You cannot send invitations and notifications</p>
                <div class="radio">
                    <div class="field">
                        <input v-validate="'required'" class="wb-radio" name="provider" type="radio" v-model="provider" id="provider_amazon"  value="amazon">
                        <label for="provider_amazon"><span class="tick"></span>{{ $t('Settings.EmailProvider_Amazon') }}</label>
                    </div>
                </div>
                <div v-if="provider === 'amazon'">
                    <div class="form-group">
                        <label>Sender email address </label>
                        <input 
                            data-vv-as="email address"
                            v-validate="'required|email'" 
                            name="senderAddress" 
                            id="senderAddress" 
                            v-model="senderAddress" 
                            type="text" 
                            class="form-control" 
                            maxlength="200" />
                        <span>{{ errors.first('settings.senderAddress') }}</span>
                    </div>
                    <h4>AWS access keys</h4>
                    <div class="form-group">
                        <label>Access key ID</label>
                        <input
                            data-
                            v-validate="'required'" 
                            class="form-control" 
                            name="awsAccessKeyId" 
                            id="awsAccessKeyId" 
                            type="text" 
                            v-model="awsAccessKeyId" 
                            maxlength="200" />
                        <span>{{ errors.first('settings.awsAccessKeyId') }}</span>
                    </div>
                    <div class="form-group">
                        <label>Secret access key</label>
                        <input 
                            v-validate="'required'" 
                            name="awsSecretAccessKey" 
                            id="awsSecretAccessKey" 
                            v-model="awsSecretAccessKey" 
                            class="form-control" 
                            type="text" 
                            maxlength="200" />
                        <span>{{ errors.first('settings.awsSecretAccessKey') }}</span>
                    </div>
                </div>
               <div class="radio">
                    <div class="field">
                        <input v-validate="'required'" class="wb-radio"  name="provider" type="radio" v-model="provider" id="provider_sendgrid"  value="sendgrid">
                        <label for="provider_sendgrid"><span class="tick"></span>{{ $t('Settings.EmailProvider_Sendgrid') }}</label>
                    </div>
                </div>
                <div  v-if="provider === 'sendgrid'">
                    <div class="form-group">
                        <label>Sender email address</label>
                        <input 
                        data-vv-as="email address"
                        v-validate="'required|email'" name="senderAddress" class="form-control" id="senderAddress" type="text" v-model="senderAddress" maxlength="200" />
                        <span>{{ errors.first('settings.senderAddress') }}</span>
                    </div>
                    <div class="form-group">
                        <label>API key</label>
                        <input v-validate="'required'" 
                        data-vv-as="API key"
                        name="sendGridApiKey" 
                        class="form-control" 
                        id="sendGridApiKey" 
                        type="text" 
                        v-model="sendGridApiKey" maxlength="200" />
                        <span>{{ errors.first('settings.sendGridApiKey') }}</span>
                    </div>

                </div>
                
                <button class="btn btn-success" type="button" :disabled="!isFormDirty" @click="save">Save</button>
            </form> 

            <form v-if="!sendGridIsNotSetUp || !awsIsNotSetUp" data-vv-scope="testEmail">
                <h4>Send test email</h4>
                <div v-if="!sendGridIsNotSetUp">
                    <div class="form-group">
                        <label>Email address</label>
                        <input data-vv-as="email" v-validate="'required|email'" name="testEmailAddress" class="form-control" id="testEmailAddress" type="text" v-model="testEmailAddress" maxlength="200" />
                        <span>{{ errors.first('testEmail.testEmailAddress') }}</span>
                    </div>
                    <button
                     class="btn btn-success" type="submit"  @click="sendTestEmail">Send test email</button>
                </div>
            </form>

            </div>
        </div>
    </HqLayout>
</template>

<script>
import Vue from "vue"

export default {
  data() {
    return {
        loading: true,
        provider: 'none',
        senderAddress: null,
        awsAccessKeyId: null,
        awsSecretAccessKey: null,
        sendGridApiKey: null,
        testEmailAddress: null
    };
  },
  async created() {
    const response = await this.$http.get(this.$config.model.api.getSettings);
    const settings = response.data || {};
    this.loading = false;
    this.provider = (settings.provider|| "").toLocaleLowerCase();
    this.senderAddress = settings.senderAddress;
    this.awsAccessKeyId = settings.awsAccessKeyId;
    this.awsSecretAccessKey = settings.awsSecretAccessKey;
    this.sendGridApiKey = settings.sendGridApiKey;
  },
  computed: {
    isFormDirty() {
        const keys = Object.keys((this.fields || {}).$settings || {});
        return keys.some(key => this.fields.$settings[key].dirty || this.fields.$settings[key].changed);
    },
    sendGridIsNotSetUp(){
        return this.sendGridApiKey==null || this.senderAddress==null;
    },
    awsIsNotSetUp(){
        return this.awsSecretAccessKey==null || this.awsAccessKeyId==null || this.senderAddress==null;
    }
  },
  methods: {
    async sendTestEmail(){
        var validationResult = await this.$validator.validateAll('testEmail');
        if (validationResult)
        {
             this.$http.post(this.$config.model.api.sendTestEmail, {
                    email: this.testEmailAddress
                });  
        }
    },
    async save(){
        var validationResult = await this.$validator.validateAll('settings');
        if (validationResult)
        {
            const settings = {
                provider: this.provider,
                senderAddress: this.senderAddress,
                awsAccessKeyId: this.awsAccessKeyId,
                awsSecretAccessKey: this.awsSecretAccessKey,
                sendGridApiKey: this.sendGridApiKey
            };
            var response = await this.$http.post(this.$config.model.api.updateSettings, settings);
            if (response.status != 200)
            {

            }
        }else{
            var fieldName = this.errors.items[0].field;
            const $firstFieldWithError = $("#"+fieldName);
            $firstFieldWithError.focus();
        }        
    }
  }
};
</script>