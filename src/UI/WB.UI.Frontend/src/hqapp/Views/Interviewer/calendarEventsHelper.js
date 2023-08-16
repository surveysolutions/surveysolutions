import gql from 'graphql-tag'
import * as toastr from 'toastr'

const deleteCalendarEventMutation =
    gql`mutation deleteCalendarEvent($workspace: String!, $publicKey: UUID!) {
            deleteCalendarEvent(workspace: $workspace, publicKey: $publicKey) {
                publicKey
            }
        }`

const updateCalendarEventMutation =
    gql`mutation updateCalendarEvent($workspace: String!, $publicKey: UUID!, $newStart: DateTime!, $comment: String, $startTimezone: String!) {
        updateCalendarEvent(workspace: $workspace, publicKey: $publicKey, newStart: $newStart, comment: $comment, startTimezone: $startTimezone) {
                publicKey
            }
        }`

const addAssignmentCalendarEventMutation =
        gql`mutation addAssignmentCalendarEvent($workspace: String!, $assignmentId: Int!, $newStart: DateTime!, $comment: String, $startTimezone: String!) {
            addAssignmentCalendarEvent(workspace: $workspace, assignmentId: $assignmentId, newStart: $newStart, comment: $comment, startTimezone: $startTimezone) {
                    publicKey
                }
            }`

const addInterviewCalendarEventMutation =
    gql`mutation addInterviewCalendarEvent($workspace: String!, $interviewId: UUID!, $newStart: DateTime!, $comment: String, $startTimezone: String!) {
        addInterviewCalendarEvent(workspace: $workspace, interviewId: $interviewId, newStart: $newStart, comment: $comment, startTimezone: $startTimezone) {
                publicKey
            }
        }`

export function deleteCalendarEvent(apollo, variables, callback) {

    executeMutation(apollo, deleteCalendarEventMutation, variables, callback)
}

export function addAssignmentCalendarEvent(apollo, variables, callback) {

    executeMutation(apollo, addAssignmentCalendarEventMutation, variables, callback)
}

export function updateCalendarEvent(apollo, variables, callback) {

    executeMutation(apollo, updateCalendarEventMutation, variables, callback)
}
export function addInterviewCalendarEvent(apollo, variables, callback) {

    executeMutation(apollo, addInterviewCalendarEventMutation, variables, callback)
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