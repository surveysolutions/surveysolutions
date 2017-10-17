<template>
    <aside class="filters">
        <div class="wrapper-view-mode"
             style="padding-top: 98px;">
            <div class="foldback-button"
                 id="hide-filters"
                 style="margin-top: 98px;">
                <span class="arrow"></span>
                <span class="arrow"></span>
                <span class="glyphicon glyphicon-tasks"
                      aria-hidden="true"></span>
            </div>
            <div class="filters-container">
                <h2>{{$config.model.key}}</h2>
                <ul class="list-unstyled about-questionnaire">
                    <li>
                        <b>Assignment #356, interview 3</b>
                    </li>
                    <li>
                        <b>conducted on
                            <span>Mar 23, 23:23 GMT+3</span>
                        </b>
                    </li>
                    <li>
                        <b>by
                            <a href="#"
                               target="_blank"
                               class="interviewer">enumer52</a>
                        </b>
                    </li>
                </ul>
                <div class="filter-actions-block">
                    <button type="button"
                            class="btn btn-success">Approve</button>
                    <button type="button"
                            class="btn btn-default btn-lg reject">Reject</button>

                    <button type="button"
                            class="btn btn-link"
                            data-toggle="modal"
                            data-target="#statuses">History</button>
                </div>
            </div>
            <div class="filters-container">
                <h4>Filter Questions</h4>
                <div class="block-filter separate-filters-group">
                    <div class="form-group">
                        <input class="checkbox-filter"
                               id="flagged"
                               type="checkbox">
                        <label for="flagged">
                            <span class="tick"></span>Flagged
                        </label>
                    </div>
                    <div class="form-group">
                        <input class="checkbox-filter"
                               id="with-comments"
                               type="checkbox">
                        <label for="with-comments">
                            <span class="tick"></span>With comments
                        </label>
                    </div>
                </div>
                <div class="block-filter separate-filters-group">
                    <div class="form-group">
                        <input class="checkbox-filter"
                               id="Invalid"
                               type="checkbox">
                        <label for="Invalid">
                            <span class="tick"></span>Invalid
                        </label>
                    </div>
                    <div class="form-group">
                        <input class="checkbox-filter"
                               id="valid"
                               type="checkbox">
                        <label for="valid">
                            <span class="tick"></span>Valid
                        </label>
                    </div>
                </div>
                <div class="block-filter separate-filters-group">
                    <div class="form-group">
                        <input class="checkbox-filter"
                               id="answered"
                               type="checkbox">
                        <label for="answered">
                            <span class="tick"></span>Answered
                        </label>
                    </div>
                    <div class="form-group">
                        <input class="checkbox-filter"
                               id="unanswered"
                               type="checkbox">
                        <label for="unanswered">
                            <span class="tick"></span>Unanswered
                        </label>
                    </div>
                </div>
                <div class="block-filter">
                    <div class="form-group">
                        <input class="checkbox-filter"
                               id="for-supervisor"
                               type="checkbox">
                        <label for="for-supervisor">
                            <span class="tick"></span>For supervisor
                        </label>
                    </div>
                    <div class="form-group">
                        <input class="checkbox-filter"
                               id="for-interviewer"
                               type="checkbox">
                        <label for="for-interviewer">
                            <span class="tick"></span>For interviewer
                        </label>
                    </div>
                </div>
            </div>
            <div class="preset-filters-container">
                <div class="block-filter">
                    <button type="button"
                            class="btn btn-link btn-looks-like-link reset-filters">View all (reset all filters)
                    </button>
                </div>
            </div>
        </div>
    </aside>
</template>
<script>
export default {

}
</script>
