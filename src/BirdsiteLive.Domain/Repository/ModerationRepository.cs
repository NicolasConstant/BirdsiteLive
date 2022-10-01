﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Domain.Tools;

namespace BirdsiteLive.Domain.Repository
{
    public interface IModerationRepository
    {
        ModerationTypeEnum GetModerationType(ModerationEntityTypeEnum type);
        ModeratedTypeEnum CheckStatus(ModerationEntityTypeEnum type, string entity);
    }

    public class ModerationRepository : IModerationRepository
    {
        private readonly Regex[] _followersAllowListing;
        private readonly Regex[] _followersBlockListing;
        private readonly Regex[] _twitterAccountsAllowListing;
        private readonly Regex[] _twitterAccountsBlockListing;

        private readonly Dictionary<ModerationEntityTypeEnum, ModerationTypeEnum> _modMode =
            new Dictionary<ModerationEntityTypeEnum, ModerationTypeEnum>();

        #region Ctor
        public ModerationRepository(ModerationSettings settings)
        {
            var parsedFollowersAllowListing = PatternsParser.Parse(settings.FollowersAllowListing);
            var parsedFollowersBlockListing = PatternsParser.Parse(settings.FollowersBlockListing);
            var parsedTwitterAccountsAllowListing = PatternsParser.Parse(settings.TwitterAccountsAllowListing);
            var parsedTwitterAccountsBlockListing = PatternsParser.Parse(settings.TwitterAccountsBlockListing);

            _followersAllowListing = parsedFollowersAllowListing
                .Select(x => ModerationRegexParser.Parse(ModerationEntityTypeEnum.Follower, x))
                .ToArray();
            _followersBlockListing = parsedFollowersBlockListing
                .Select(x => ModerationRegexParser.Parse(ModerationEntityTypeEnum.Follower, x))
                .ToArray();
            _twitterAccountsAllowListing = parsedTwitterAccountsAllowListing
                .Select(x => ModerationRegexParser.Parse(ModerationEntityTypeEnum.TwitterAccount, x))
                .ToArray();
            _twitterAccountsBlockListing = parsedTwitterAccountsBlockListing
                .Select(x => ModerationRegexParser.Parse(ModerationEntityTypeEnum.TwitterAccount, x))
                .ToArray();

            // Set Follower moderation politic
            if (_followersAllowListing.Any())
                _modMode.Add(ModerationEntityTypeEnum.Follower, ModerationTypeEnum.AllowListing);
            else if (_followersBlockListing.Any())
                _modMode.Add(ModerationEntityTypeEnum.Follower, ModerationTypeEnum.BlockListing);
            else
                _modMode.Add(ModerationEntityTypeEnum.Follower, ModerationTypeEnum.None);

            // Set Twitter account moderation politic
            if (_twitterAccountsAllowListing.Any())
                _modMode.Add(ModerationEntityTypeEnum.TwitterAccount, ModerationTypeEnum.AllowListing);
            else if (_twitterAccountsBlockListing.Any())
                _modMode.Add(ModerationEntityTypeEnum.TwitterAccount, ModerationTypeEnum.BlockListing);
            else
                _modMode.Add(ModerationEntityTypeEnum.TwitterAccount, ModerationTypeEnum.None);
        }
        #endregion

        public ModerationTypeEnum GetModerationType(ModerationEntityTypeEnum type)
        {
            return _modMode[type];
        }

        public ModeratedTypeEnum CheckStatus(ModerationEntityTypeEnum type, string entity)
        {
            if (_modMode[type] == ModerationTypeEnum.None) return ModeratedTypeEnum.None;

            switch (type)
            {
                case ModerationEntityTypeEnum.Follower:
                    return ProcessFollower(entity);
                case ModerationEntityTypeEnum.TwitterAccount:
                    return ProcessTwitterAccount(entity);
            }

            throw new NotImplementedException($"Type {type} is not supported");
        }
        
        private ModeratedTypeEnum ProcessFollower(string entity)
        {
            var politic = _modMode[ModerationEntityTypeEnum.Follower];

            switch (politic)
            {
                case ModerationTypeEnum.None:
                    return ModeratedTypeEnum.None;
                case ModerationTypeEnum.BlockListing:
                    if (_followersBlockListing.Any(x => x.IsMatch(entity)))
                        return ModeratedTypeEnum.BlockListed;
                    return ModeratedTypeEnum.None;
                case ModerationTypeEnum.AllowListing:
                    if (_followersAllowListing.Any(x => x.IsMatch(entity)))
                        return ModeratedTypeEnum.AllowListed;
                    return ModeratedTypeEnum.None;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ModeratedTypeEnum ProcessTwitterAccount(string entity)
        {
            var politic = _modMode[ModerationEntityTypeEnum.TwitterAccount];

            switch (politic)
            {
                case ModerationTypeEnum.None:
                    return ModeratedTypeEnum.None;
                case ModerationTypeEnum.BlockListing:
                    if (_twitterAccountsBlockListing.Any(x => x.IsMatch(entity)))
                        return ModeratedTypeEnum.BlockListed;
                    return ModeratedTypeEnum.None;
                case ModerationTypeEnum.AllowListing:
                    if (_twitterAccountsAllowListing.Any(x => x.IsMatch(entity)))
                        return ModeratedTypeEnum.AllowListed;
                    return ModeratedTypeEnum.None;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum ModerationEntityTypeEnum
    {
        Unknown = 0,
        Follower = 1,
        TwitterAccount = 2
    }

    public enum ModerationTypeEnum
    {
        None = 0,
        BlockListing = 1,
        AllowListing = 2
    }

    public enum ModeratedTypeEnum
    {
        None = 0,
        BlockListed = 1,
        AllowListed = 2
    }
}