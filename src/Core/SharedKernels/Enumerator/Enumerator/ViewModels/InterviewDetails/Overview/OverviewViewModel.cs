﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewViewModel : MvxViewModel
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IImageFileStorage fileStorage;
        private readonly IViewModelNavigationService navigationService;
        private readonly IAudioService audioService;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly DynamicTextViewModel nameViewModel;

        public OverviewViewModel(IStatefulInterviewRepository interviewRepository,
            IImageFileStorage fileStorage,
            IViewModelNavigationService navigationService,
            IAudioService audioService,
            IAudioFileStorage audioFileStorage,
            DynamicTextViewModel nameViewModel)
        {
            this.interviewRepository = interviewRepository;
            this.fileStorage = fileStorage;
            this.navigationService = navigationService;
            this.audioService = audioService;
            this.audioFileStorage = audioFileStorage;
            this.nameViewModel = nameViewModel;
        }

        public void Configure(string interviewId)
        {
            var interview = interviewRepository.Get(interviewId);
            var sections = interview.GetEnabledSections().Select(x => x.Identity).ToImmutableHashSet();

            var interviewEntities = interview.GetUnderlyingInterviewerEntities();

            this.Name = nameViewModel;
            this.Name.InitAsStatic(UIResources.Interview_Overview_Name);
            this.Items = interviewEntities.Where(x => interview.IsEnabled(x)).Select(x => BuildOverviewNode(x, interview, sections)).ToList();
        }

        private OverviewNode BuildOverviewNode(Identity interviewerEntityIdentity,
            IStatefulInterview interview,
            ICollection<Identity> sections)
        {
            var question = interview.GetQuestion(interviewerEntityIdentity);

            if (question != null)
            {
                if (question.IsMultimedia)
                {
                    return new OverviewMultimediaQuestionViewModel(question, fileStorage, navigationService);
                }

                if (question.IsAudio)
                {
                    return new OverviewAudioQuestionViewModel(question, audioFileStorage, audioService);
                }

                return new OverviewQuestion(question);
            }

            var staticText = interview.GetStaticText(interviewerEntityIdentity);
            if (staticText != null)
            {
                return new OverviewStaticText(staticText)
                {
                    Id = staticText.Identity.ToString(),
                    Title = staticText.Title.Text
                };
            }

            var group = interview.GetGroup(interviewerEntityIdentity);
            if (group != null)
            {
                if (sections.Contains(group.Identity))
                {
                    return new OverviewSection(group)
                    {
                        Id = group.Identity.ToString(),
                        Title = group.Title.Text
                    };
                }
                return new OverviewGroup(group)
                {
                    Id = group.Identity.ToString(),
                    Title = group.Title.Text
                };
            }

            throw new NotSupportedException($"Display of {interviewerEntityIdentity} entity is not supported");
        }

        public List<OverviewNode> Items { get; private set; }

        public DynamicTextViewModel Name { get; private set; }
    }
}

