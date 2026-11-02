import { gql, gqlRequest } from '~/hqapp/api/graphql'
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

export function deleteCalendarEvent(variables, callback) {
    executeMutation(deleteCalendarEventMutation, variables, callback)
}

export function addAssignmentCalendarEvent(variables, callback) {
    executeMutation(addAssignmentCalendarEventMutation, variables, callback)
}

export function updateCalendarEvent(variables, callback) {
    executeMutation(updateCalendarEventMutation, variables, callback)
}
export function addInterviewCalendarEvent(variables, callback) {
    executeMutation(addInterviewCalendarEventMutation, variables, callback)
}

function executeMutation(mutation, variables, callback) {
    gqlRequest(mutation, variables).then(() => {
        callback()
    }).catch(err => {
        console.error(err)
        toastr.error(err.message.toString())
    })
}