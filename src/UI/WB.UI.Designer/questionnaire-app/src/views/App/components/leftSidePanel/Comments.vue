<template>
    Comments

    <!--div class="comments">
        <perfect-scrollbar class="scroller">
            <h3>
                <span ng-i18next="[i18next]({count: commentThreads.length})SideBarCommentsCounter"></span>
            </h3>
            <div class="empty-list" ng-show="commentThreads.length == 0">
                <p ng-i18next>SideBarEmptyCommentsLine</p>
            </div>
            <ul ng-model="commentThreads">
                <li class="comment-thread" ng-repeat="commentThread in commentThreads">
                    <a class="reference-item" href="javascript:void(0);"
                        @click="showCommentsAndNavigateTo(commentThread.entity)">
                        <span ng-if="commentThread.entity.type == 'Question'"
                            class="icon {{commentThread.entity.questionType}} "></span>
                        <span
                            ng-if="commentThread.entity.type !== 'Question' && commentThread.entity.type !== 'Group' && commentThread.entity.type !== 'Roster'"
                            class="icon icon-{{commentThread.entity.type.toLowerCase()}}"></span>
                        <span class="title">{{ commentThread.entity.title | escape }}</span>
                        <span class="variable" ng-bind-html="commentThread.entity.variable || '&nbsp;'">&nbsp;</span>
                    </a>
                    <div class="comments-in-thread">
                        <ul>
                            <li class="comment" :class="{ resolved: comment.isResolved }"
                                ng-repeat="comment in commentThread.comments">
                                <span class="author">{{ comment.userEmail }}</span>
                                <span class="date">{{ comment.date }}</span>
                                <p class="comment-text">{{ comment.comment }}</p>
                            </li>
                        </ul>
                        <div ng-if="commentThread.resolvedComments.length > 0">
                            <a href="javascript:void(0);" class="show-more" @click="commentThread.toggleResolvedComments()">
                                <span ng-hide="commentThread.resolvedAreExpanded"
                                    ng-i18next="[i18next]({count: commentThread.resolvedComments.length})ViewResolvedCommentsCounter"></span>
                                <span ng-show="commentThread.resolvedAreExpanded" ng-i18next>HideResolvedComments</span>
                            </a>
                            <ul ng-show="commentThread.resolvedAreExpanded">
                                <li class="comment" :class="{ resolved: comment.isResolved }"
                                    ng-repeat="comment in commentThread.resolvedComments">
                                    <span class="author">{{ comment.userEmail }}</span>
                                    <span class="date">{{ comment.date }}</span>
                                    <p class="comment-text">{{ comment.comment }}</p>
                                </li>
                            </ul>
                        </div>
                    </div>
                </li>
            </ul>
        </perfect-scrollbar>
    </div-->
</template>
  
<script>
export default {
    name: 'Comments',
    props: {},
    data() {
        return {

        }
    }
}
</script>
  