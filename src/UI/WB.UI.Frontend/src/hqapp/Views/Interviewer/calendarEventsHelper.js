import gql from 'graphql-tag'
import * as toastr from 'toastr'

const deleteCalendarEventMutation =
    gql`mutation deleteCalendarEvent($workspace: String!, $publicKey: Uuid!) {
            deleteCalendarEvent(workspace: $workspace, publicKey: $publicKey) {
                publicKey
            }
        }`

const addOrUpdateCalendarEventMutation =
    gql`mutation addOrUpdateCalendarEvent($workspace: String!, $publicKey: Uuid, $interviewId: Uuid, $interviewKey: String, $assignmentId: Int!, $newStart: DateTime!, $comment: String, $startTimezone: String) {
            addOrUpdateCalendarEvent(workspace: $workspace, publicKey: $publicKey, interviewId: $interviewId, interviewKey: $interviewKey, assignmentId: $assignmentId, newStart: $newStart, comment: $comment, startTimezone:$startTimezone) {
                publicKey
            }
        }`

export function deleteCalendarEvent(apollo, variables, callback) {

    executeMutation(apollo, deleteCalendarEventMutation, variables, callback)
}

export function addOrUpdateCalendarEvent(apollo, variables, callback) {

    executeMutation(apollo, addOrUpdateCalendarEventMutation, variables, callback)
}

function executeMutation(apollo, mutation, variables, callback) {
    apollo.mutate({
        mutation: mutation,
        variables: variables,
    }).then(response => {
        callback()
    }).catch(err => {
        console.error(err)
        toastr.error(err.message.toString())
    })
}