﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.P2PNetwork.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Used in each <see cref="Node"/> to show its status at all times.
    /// </summary>
    public class NodeStatus : INodeStatus, INotifyPropertyChanged
    {
        private int _v;
        /// <summary>
        /// Returns the violation score of this node
        /// </summary>
        public int Violation
        {
            get => _v;
            set
            {
                _v = value;
                if (ShouldDisconnect)
                {
                    RaiseDisconnectEvent();
                }
            }
        }
        private const int SmallV = 10;
        private const int MediumV = 20;
        private const int BigV = 50;
        private const int DisconnectThreshold = 100;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <inheritdoc/>
        public event EventHandler DisconnectEvent;

        private IPAddress _ip;
        /// <inheritdoc/>
        public IPAddress IP
        {
            get => _ip;
            set => SetField(ref _ip, value);
        }

        private int _protVer;
        /// <inheritdoc/>
        public int ProtocolVersion
        {
            get => _protVer;
            set => SetField(ref _protVer, value);
        }

        private NodeServiceFlags _servs;
        /// <inheritdoc/>
        public NodeServiceFlags Services
        {
            get => _servs;
            set => SetField(ref _servs, value);
        }

        private ulong _nonce;
        /// <inheritdoc/>
        public ulong Nonce
        {
            get => _nonce;
            set => SetField(ref _nonce, value);
        }

        private string _ua;
        /// <inheritdoc/>
        public string UserAgent
        {
            get => _ua;
            set => SetField(ref _ua, value);
        }

        private int _height;
        /// <inheritdoc/>
        public int StartHeight
        {
            get => _height;
            set => SetField(ref _height, value);
        }

        private bool _relay;
        /// <inheritdoc/>
        public bool Relay
        {
            get => _relay;
            set => SetField(ref _relay, value);
        }

        private ulong _fee;
        /// <inheritdoc/>
        public ulong FeeFilter
        {
            get => _fee;
            set => SetField(ref _fee, value);
        }

        private bool _cmpt;
        /// <inheritdoc/>
        public bool SendCompact
        {
            get => _cmpt;
            set => SetField(ref _cmpt, value);
        }

        private ulong _cmptVer;
        /// <inheritdoc/>
        public ulong SendCompactVer
        {
            get => _cmptVer;
            set
            {
                if (_cmptVer < value)
                    SetField(ref _cmptVer, value);
            }
        }

        private DateTime _lastSeen;
        /// <inheritdoc/>
        public DateTime LastSeen
        {
            get => _lastSeen;
            private set => SetField(ref _lastSeen, value);
        }

        private HandShakeState _handShake = HandShakeState.None;
        /// <inheritdoc/>
        public HandShakeState HandShake
        {
            get => _handShake;
            set => SetField(ref _handShake, value);
        }

        private bool _isDead = false;
        /// <inheritdoc/>
        public bool IsDisconnected
        {
            get => _isDead;
            set
            {
                if (SetField(ref _isDead, value) && value)
                {
                    RaiseDisconnectEvent();
                }
            }
        }


        /// <inheritdoc/>
        public bool ShouldDisconnect => Violation >= DisconnectThreshold;

        private void RaiseDisconnectEvent() => DisconnectEvent?.Invoke(this, EventArgs.Empty);

        /// <inheritdoc/>
        public void UpdateTime() => LastSeen = DateTime.Now;
        /// <inheritdoc/>
        public void AddBigViolation() => Violation += BigV;
        /// <inheritdoc/>
        public void AddMediumViolation() => Violation += MediumV;
        /// <inheritdoc/>
        public void AddSmallViolation() => Violation += SmallV;


        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event using the given property name.
        /// The event is only invoked if data binding is used
        /// </summary>
        /// <param name="propertyName">The Name of the property that is changing.</param>
        private void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            else
            {
                field = value;
                RaisePropertyChanged(propertyName);
                return true;
            }
        }
    }
}
