﻿// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;

namespace Autarkysoft.Bitcoin.Blockchain.Scripts.Operations
{
    /// <summary>
    /// Base (abstract) class for conditional operations (covering <see cref="OP.IF"/>, <see cref="OP.NotIf"/>, 
    /// <see cref="OP.ELSE"/> and <see cref="OP.EndIf"/> OPs).
    /// </summary>
    public abstract class IfElseOp : BaseOperation
    {
        /// <summary>
        /// Each IF operation pops an item from the stack first and converts it to a boolean. 
        /// <see cref="OP.IF"/> expressions run if that item was true while 
        /// <see cref="OP.NotIf"/> expressions run if that item was false.
        /// </summary>
        protected bool runWithTrue;

        /// <summary>
        /// The main operations under the <see cref="OP.IF"/> or <see cref="OP.NotIf"/> OP.
        /// </summary>
        protected internal IOperation[] mainOps;
        /// <summary>
        /// The "else" operations under the <see cref="OP.ELSE"/> op.
        /// </summary>
        protected internal IOperation[] elseOps;


        /// <summary>
        /// Runs all the operations in main or else list. Return value indicates success.
        /// </summary>
        /// <param name="opData">Stack to use</param>
        /// <param name="error">Error message (null if sucessful, otherwise will contain information about the failure)</param>
        /// <returns>True if operation was successful, false if otherwise</returns>
        public override bool Run(IOpData opData, out string error)
        {
            if (opData.ItemCount < 1)
            {
                error = "Invalid number of elements in stack.";
                return false;
            }

            // TODO: check what this part is 
            // https://github.com/bitcoin/bitcoin/blob/a822a0e4f6317f98cde6f0d5abe952b4e8992ac9/src/script/interpreter.cpp#L478-L483
            // probably have to include it in wherver we run these operations. (check consensus related to witness,...)

            // Remove top stack item and convert it to bool
            // OP_IF: runs if True
            // OP_NOTIF: runs if false
            if (IsNotZero(opData.Pop()) == runWithTrue)
            {
                foreach (var op in mainOps)
                {
                    if (!op.Run(opData, out error))
                    {
                        return false;
                    }
                }
            }
            else if (elseOps != null && elseOps.Length != 0)
            {
                foreach (var op in elseOps)
                {
                    if (!op.Run(opData, out error))
                    {
                        return false;
                    }
                }
            }

            error = null;
            return true;
        }


        /// <summary>
        /// Writes this operation's data to the given stream.
        /// Used by <see cref="IDeserializable.Serialize(FastStream)"/> methods 
        /// (not to be confused with what <see cref="Run(IOpData, out string)"/> does).
        /// </summary>
        /// <param name="stream">Stream to use</param>
        public void WriteToStream(FastStream stream)
        {
            // Start with OP_IF or OP_NotIf
            stream.Write((byte)OpValue);
            foreach (var op in mainOps)
            {
                if (op is PushDataOp push)
                {
                    push.WriteToStream(stream);
                }
                else if (op is IfElseOp conditional)
                {
                    conditional.WriteToStream(stream);
                }
                else
                {
                    stream.Write((byte)op.OpValue);
                }
            }

            // Continue with OP_ELSE if it exists
            if (elseOps != null && elseOps.Length != 0)
            {
                stream.Write((byte)OP.ELSE);
                foreach (var op in elseOps)
                {
                    if (op is PushDataOp push)
                    {
                        push.WriteToStream(stream);
                    }
                    else if (op is IfElseOp conditional)
                    {
                        conditional.WriteToStream(stream);
                    }
                    else
                    {
                        stream.Write((byte)op.OpValue);
                    }
                }
            }

            // End with OP_EndIf
            stream.Write((byte)OP.EndIf);
        }


        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the specified object is equal to the current object, flase if otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is IfElseOp op)
            {
                if (op.OpValue == OpValue)
                {
                    if (op.mainOps.Length == mainOps.Length)
                    {
                        for (int i = 0; i < mainOps.Length; i++)
                        {
                            if (!op.mainOps[i].Equals(mainOps[i]))
                            {
                                return false;
                            }
                        }

                        if (op.elseOps == null && elseOps != null && elseOps.Length != 0 ||
                            op.elseOps != null && elseOps == null && op.elseOps.Length != 0 ||
                            op.elseOps != null && elseOps != null && op.elseOps.Length != elseOps.Length)
                        {
                            return false;
                        }
                        if (op.elseOps != null && elseOps != null && op.elseOps.Length == elseOps.Length)
                        {
                            for (int i = 0; i < elseOps.Length; i++)
                            {
                                if (!op.elseOps[i].Equals(elseOps[i]))
                                {
                                    return false;
                                }
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code</returns>
        public override int GetHashCode()
        {
            int result = 17;
            foreach (var item in mainOps)
            {
                result ^= item.GetHashCode();
            }
            if (elseOps != null)
            {
                foreach (var item in elseOps)
                {
                    result ^= item.GetHashCode();
                }
            }

            return result;
        }
    }





    /// <summary>
    /// Operation to run a set of operations (if expressions) if preceeded by True (anything except zero).
    /// </summary>
    public class IFOp : IfElseOp
    {
        /// <summary>
        /// Initializes a new instance of <see cref="IFOp"/> for internal use
        /// </summary>
        internal IFOp()
        {
            runWithTrue = true;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="IFOp"/> with the given <see cref="IOperation"/> arrays.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ifBlockOps">The main array of operations to run after <see cref="OP.IF"/></param>
        /// <param name="elseBlockOps">
        /// [Default value = null] The alternative set of operations to run if the previous expresions didn't run.
        /// </param>
        public IFOp(IOperation[] ifBlockOps, IOperation[] elseBlockOps = null)
        {
            if (ifBlockOps is null || ifBlockOps.Length == 0)
                throw new ArgumentNullException(nameof(ifBlockOps), "The if block operations can not be null or empty.");

            runWithTrue = true;
            mainOps = ifBlockOps;
            elseOps = elseBlockOps;
        }

        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.IF;
    }


    /// <summary>
    /// Operation to run a set of operations (NotIf expressions) if preceeded by False (an empty array aka OP_0).
    /// </summary>
    public class NotIfOp : IfElseOp
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NotIfOp"/> for internal use
        /// </summary>
        internal NotIfOp()
        {
            runWithTrue = false;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="IFOp"/> with the given <see cref="IOperation"/> arrays.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="ifBlockOps">The main array of operations to run after <see cref="OP.IF"/></param>
        /// <param name="elseBlockOps">
        /// [Default value = null] The alternative set of operations to run if the previous expresions didn't run.
        /// </param>
        public NotIfOp(IOperation[] ifBlockOps, IOperation[] elseBlockOps)
        {
            if (ifBlockOps is null || ifBlockOps.Length == 0)
                throw new ArgumentNullException(nameof(ifBlockOps), "The if block operations can not be null or empty.");

            runWithTrue = false;
            mainOps = ifBlockOps;
            elseOps = elseBlockOps;
        }

        /// <inheritdoc cref="IOperation.OpValue"/>
        public override OP OpValue => OP.NotIf;
    }
}