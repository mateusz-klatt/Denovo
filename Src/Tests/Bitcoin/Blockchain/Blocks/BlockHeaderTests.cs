﻿// Autarkysoft Tests
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin;
using Autarkysoft.Bitcoin.Blockchain.Blocks;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests.Bitcoin.Blockchain.Blocks
{
    public class BlockHeaderTests
    {
        [Fact]
        public void Constructor_NullExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() => new BlockHeader(1, null, new byte[32], 123, 0x1d00ffffU, 0));
            Assert.Throws<ArgumentNullException>(() => new BlockHeader(1, new byte[32], null, 123, 0x1d00ffffU, 0));
        }

        public static IEnumerable<object[]> GetCtorOutOfRangeCases()
        {
            yield return new object[] { new byte[31], new byte[32] };
            yield return new object[] { new byte[32], new byte[33] };
        }
        [Theory]
        [MemberData(nameof(GetCtorOutOfRangeCases))]
        public void Constructor_OutOfRangeExceptionTest(byte[] header, byte[] merkle)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BlockHeader(1, header, merkle, 123, 0x1d00ffffU, 0));
        }


        // Block #622051
        internal static BlockHeader GetSampleBlockHeader()
        {
            return new BlockHeader()
            {
                Version = 0x3fffe000,
                PreviousBlockHeaderHash = Helper.HexToBytes("97e4833c21eab4dfc5153eadc3b33701c8420ea1310000000000000000000000"),
                MerkleRootHash = Helper.HexToBytes("afbdfb477c57f95a59a9e7f1d004568c505eb7e70fb73fb0d6bb1cca0fb1a7b7"),
                BlockTime = 0x5e71b1c6,
                NBits = 0x17110119,
                Nonce = 0x2a436a69
            };
        }

        internal static string GetSampleBlockHex() => "0000000000000000000d558fdcdde616702d1f91d6c8567a89be99ff9869012d";
        internal static byte[] GetSampleBlockHash() => Helper.HexToBytes(GetSampleBlockHex(), true);
        internal static byte[] GetSampleBlockHeaderBytes() => Helper.HexToBytes("00e0ff3f97e4833c21eab4dfc5153eadc3b33701c8420ea1310000000000000000000000afbdfb477c57f95a59a9e7f1d004568c505eb7e70fb73fb0d6bb1cca0fb1a7b7c6b1715e19011117696a432a");


        [Fact]
        public void SerializeTest()
        {
            BlockHeader hd = GetSampleBlockHeader();

            FastStream stream = new FastStream();
            hd.Serialize(stream);

            byte[] expected = GetSampleBlockHeaderBytes();

            Assert.Equal(expected, stream.ToByteArray());
            Assert.Equal(expected, hd.Serialize());
        }

        [Fact]
        public void TryDeserializeTest()
        {
            BlockHeader blk = new BlockHeader();
            bool b = blk.TryDeserialize(new FastStreamReader(GetSampleBlockHeaderBytes()), out string error);
            BlockHeader expected = GetSampleBlockHeader();

            Assert.True(b, error);
            Assert.Null(error);
            Assert.Equal(expected.Version, blk.Version);
            Assert.Equal(expected.PreviousBlockHeaderHash, blk.PreviousBlockHeaderHash);
            Assert.Equal(expected.MerkleRootHash, blk.MerkleRootHash);
            Assert.Equal(expected.BlockTime, blk.BlockTime);
            Assert.Equal(expected.NBits, blk.NBits);
            Assert.Equal(expected.Nonce, blk.Nonce);
        }

        public static IEnumerable<object[]> GetDeserFailCases()
        {
            yield return new object[]
            {
                new byte[Constants.BlockHeaderSize -1],
                Err.EndOfStream
            };
        }
        [Theory]
        [MemberData(nameof(GetDeserFailCases))]
        public void TryDeserialize_FailTests(byte[] data, string expErr)
        {
            Block blk = new Block();
            bool b = blk.TryDeserialize(new FastStreamReader(data), out string error);

            Assert.False(b, error);
            Assert.Equal(expErr, error);
        }
    }
}
