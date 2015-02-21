var approveAll = function () {
    var total = 0;
    var commands = {};
    var config = {
        commands: {
            approveInterviewCommand: "ApproveInterviewCommand",
            hQApproveInterviewCommand: "HqApproveInterviewCommand"
        }
    };

    commands[config.commands.approveInterviewCommand] = function (args) {
        return {
            interviewId: args.interviewId,
            commentTime: new Date(),
            comment: args.comment
        };
    };


    commands[config.commands.hQApproveInterviewCommand] = function (args) {
        return {
            interviewId: args.interviewId,
            commentTime: new Date(),
            comment: args.comment
        };
    };


    var getCommand = function (commandName, args) {
        return {
            type: commandName,
            command: ko.toJSON(commands[commandName](args))
        };
    };

    var sendRequest = function (requestUrl, args, onSuccess, skipInProgressCheck) {

        var requestHeaders = {};
        requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;

        $.ajax({
            url: requestUrl,
            type: 'post',
            data: args,
            headers: requestHeaders,
            dataType: 'json'
        })
        .done(function (data) {
            console.log('approved ' + args.command);
            total--;
            console.log("left " + total);
        });
    };

    var list = ko.dataFor($("#list tbody")[0]);

    var ids = _.map(list.Items(), function (item) { return item.InterviewId() });

    _.each(ids, function (id, index) {
        console.log("approving interivew " + id);
        var command = getCommand(config.commands.approveInterviewCommand, {
            interviewId: id,
            comment: "Approved by Survey solutions support team"
        });
        total++;

        $.ajax({
            type: "POST",
            url: '/Interview/ConfirmRevalidation/' + id,
            data: {
                InterviewId: id
            },
            success: function () {
                console.log('revalidated interview ' + id);
                sendRequest('/api/CommandApi/Execute', command, function () { });
            }

        });
    });
    console.log("queued " + total);
}

