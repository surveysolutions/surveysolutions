<template>
    <div>
        <div class="filters-container">
            <h4>Filter Questions</h4>
            <div class="block-filter separate-filters-group">
                <div class="form-group">
                    <input class="checkbox-filter" id="flagged" type="checkbox">
                    <label for="flagged">
                        <span class="tick"></span>Flagged
                    </label>
                </div>
                <div class="form-group">
                    <input class="checkbox-filter" id="with-comments" type="checkbox">
                    <label for="with-comments">
                        <span class="tick"></span>With comments
                    </label>
                </div>
            </div>
            <div class="block-filter separate-filters-group">
                <div class="form-group">
                    <input class="checkbox-filter" id="Invalid" type="checkbox">
                    <label for="Invalid">
                        <span class="tick"></span>Invalid
                    </label>
                </div>
                <div class="form-group">
                    <input class="checkbox-filter" id="valid" type="checkbox">
                    <label for="valid">
                        <span class="tick"></span>Valid
                    </label>
                </div>
            </div>
            <div class="block-filter separate-filters-group">
                <div class="form-group">
                    <input class="checkbox-filter" id="answered" type="checkbox">
                    <label for="answered">
                        <span class="tick"></span>Answered
                    </label>
                </div>
                <div class="form-group">
                    <input class="checkbox-filter" id="unanswered" type="checkbox">
                    <label for="unanswered">
                        <span class="tick"></span>Unanswered
                    </label>
                </div>
            </div>
            <div class="block-filter">
                <div class="form-group">
                    <input class="checkbox-filter" id="for-supervisor" type="checkbox">
                    <label for="for-supervisor">
                        <span class="tick"></span>For supervisor
                    </label>
                </div>
                <div class="form-group">
                    <input class="checkbox-filter" id="for-interviewer" type="checkbox">
                    <label for="for-interviewer">
                        <span class="tick"></span>For interviewer
                    </label>
                </div>
            </div>
        </div>
        <div class="preset-filters-container">
            <div class="block-filter">
                <button type="button" class="btn btn-link btn-looks-like-link reset-filters">View all (reset all filters)
                </button>
                <a :href="oldPageUrl">Old interview details</a>
            </div>
        </div>
    </div>
</template>
<script>
export default {
  computed: {
    oldPageUrl() {
      return (
        window.input.settings.config.basePath +
        "Interview/Details/" +
        this.$route.params.interviewId
      );
    }
  }
};
</script>
