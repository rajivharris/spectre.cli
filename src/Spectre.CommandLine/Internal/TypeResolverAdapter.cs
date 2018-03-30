﻿using System;
using Spectre.CommandLine.Internal.Exceptions;

namespace Spectre.CommandLine.Internal
{
    internal sealed class TypeResolverAdapter : ITypeResolver
    {
        private readonly ITypeResolver _resolver;

        public TypeResolverAdapter(ITypeResolver resolver)
        {
            _resolver = resolver;
        }

        public object Resolve(Type type)
        {
            try
            {
                if (_resolver != null)
                {
                    var obj = _resolver.Resolve(type);
                    if (obj == null)
                    {
                        throw RuntimeException.CouldNotResolveType(type);
                    }
                    return obj;
                }

                // Fall back to use the activator.
                return Activator.CreateInstance(type);
            }
            catch (CommandAppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw RuntimeException.CouldNotResolveType(type, ex);
            }
        }
    }
}
