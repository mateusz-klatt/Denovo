﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Transactions;

namespace Autarkysoft.Bitcoin.Blockchain.Blocks
{
    /// <summary>
    /// Defines methods and properties that a block class implements. Inherits from <see cref="IDeserializable"/>.
    /// </summary>
    public interface IBlock : IDeserializable
    {
        /// <summary>
        /// This block's height (set before verification using the blockchain tip and the
        /// <see cref="BlockHeader.PreviousBlockHeaderHash"/>)
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// This block's raw byte size (normally set during deserialization)
        /// </summary>
        int BlockSize { get; set; }

        /// <summary>
        /// The block header
        /// </summary>
        BlockHeader Header { get; set; }

        /// <summary>
        /// List of transactions in this block
        /// </summary>
        ITransaction[] TransactionList { get; set; }


        /// <summary>
        /// Returns hash of this block using the defined hash function.
        /// </summary>
        /// <returns>Block hash</returns>
        byte[] GetBlockHash();

        /// <summary>
        /// Returns hash of this block as a base-16 encoded string.
        /// </summary>
        /// <returns>Base-16 encoded block hash</returns>
        string GetBlockID();

        /// <summary>
        /// Returns merkle root of this block using the list of transactions.
        /// </summary>
        /// <returns>Merkle root</returns>
        byte[] ComputeMerkleRoot();

        /// <summary>
        /// Returns merkle root hash of witnesses in this block using the list of transactions.
        /// </summary>
        /// <param name="commitment">32 byte witness commitment</param>
        /// <returns>Merkle root</returns>
        byte[] ComputeWitnessMerkleRoot(byte[] commitment);
    }
}
