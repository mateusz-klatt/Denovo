﻿// Denovo
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Denovo.MVVM;
using System;

namespace Denovo.Models
{
    public class Configuration : InpcBase
    {
        public Configuration()
        {
            SetPeerList();
        }

        public Configuration(NetworkType network)
        {
            Network = network;
        }


        public NetworkType Network { get; }

        private ClientType _clientType;
        public ClientType SelectedClientType
        {
            get => _clientType;
            set => SetField(ref _clientType, value);
        }

        private int _prSize;
        public int PrunedSize
        {
            get => _prSize;
            set => SetField(ref _prSize, value);
        }


        private bool _listen;
        public bool AcceptIncoming
        {
            get => _listen;
            set => SetField(ref _listen, value);
        }


        private PeerDiscoveryOption _selDiscoverOpt;
        public PeerDiscoveryOption SelectedPeerDiscoveryOption
        {
            get => _selDiscoverOpt;
            set
            {
                if (SetField(ref _selDiscoverOpt, value))
                {
                    SetPeerList();
                }
            }
        }


        public static readonly string[] DnsMain = new string[]
        {
            "seed.bitcoin.sipa.be", // Pieter Wuille, only supports x1, x5, x9, and xd
            "dnsseed.bluematt.me", // Matt Corallo, only supports x9
            "dnsseed.bitcoin.dashjr.org", // Luke Dashjr
            "seed.bitcoinstats.com", // Christian Decker, supports x1 - xf
            "seed.bitcoin.jonasschnelli.ch", // Jonas Schnelli, only supports x1, x5, x9, and xd
            "seed.btc.petertodd.org", // Peter Todd, only supports x1, x5, x9, and xd
            "seed.bitcoin.sprovoost.nl", // Sjors Provoost
            "dnsseed.emzy.de", // Stephan Oeste
            "seed.bitcoin.wiz.biz", // Jason Maurice
        };

        public static readonly string[] DnsTest = new string[]
        {
            "testnet-seed.bitcoin.jonasschnelli.ch",
            "seed.tbtc.petertodd.org",
            "seed.testnet.bitcoin.sprovoost.nl",
            "testnet-seed.bluematt.me",
        };

        private string GetDnsList()
        {
            return Network switch
            {
                NetworkType.MainNet => string.Join(Environment.NewLine, DnsMain),
                NetworkType.TestNet => string.Join(Environment.NewLine, DnsTest),
                NetworkType.RegTest => "Not defined.",
                _ => "Not defined."
            };
        }
        private void SetPeerList()
        {
            PeerList = SelectedPeerDiscoveryOption switch
            {
                PeerDiscoveryOption.DNS => GetDnsList(),
                PeerDiscoveryOption.CustomIP => "192.168.1.1",
                _ => "Not defined!",
            };
        }

        private string _peers;
        [DependsOnProperty(nameof(SelectedPeerDiscoveryOption))]
        public string PeerList
        {
            get => _peers;
            set => SetField(ref _peers, value);
        }
    }
}
