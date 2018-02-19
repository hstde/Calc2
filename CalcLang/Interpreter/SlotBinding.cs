using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public class SlotBinding : Binding
    {
        public SlotInfo Slot;
        public ScopeInfo FromScope;
        public int SlotIndex;
        public int StaticScopeIndex;
        public Ast.AstNode FromNode;

        public SlotBinding(SlotInfo slot, Ast.AstNode fromNode, ScopeInfo fromScope) : base(slot.Name, BindingTargetType.Slot)
        {
            Slot = slot;
            FromNode = fromNode;
            FromScope = fromScope;
            SlotIndex = slot.Index;
            StaticScopeIndex = Slot.ScopeInfo.StaticIndex;
            SetupAccessorMethods();
        }

        private void SetupAccessorMethods()
        {
            if (Slot.ScopeInfo.StaticIndex >= 0)
            {
                GetValueRef = FastGetStaticValue;
                SetValueRef = SetStatic;
                return;
            }
            var levelDiff = Slot.ScopeInfo.Level - FromScope.Level;
            switch (levelDiff)
            {
                case 0:
                    if (Slot.Type == SlotType.Value)
                    {
                        GetValueRef = FastGetCurrentScopeValue;
                        SetValueRef = SetCurrentScopeValue;
                    }
                    else
                    {
                        GetValueRef = FastGetCurrentScopeParameter;
                        SetValueRef = SetCurrentScopeParameter;
                    }
                    return;
                case 1:
                    if (Slot.Type == SlotType.Value)
                    {
                        GetValueRef = GetImmediateParentScopeValue;
                        SetValueRef = SetImmediateParentScopeValue;
                    }
                    else
                    {
                        GetValueRef = GetImmediateParentScopeParameter;
                        SetValueRef = SetImmediateParentScopeParameter;
                    }
                    return;
                default:
                    if (Slot.Type == SlotType.Value)
                    {
                        GetValueRef = GetParentScopeValue;
                        SetValueRef = SetParentScopeValue;
                    }
                    else
                    {
                        GetValueRef = GetParentScopeParameter;
                        SetValueRef = SetParentScopeParameter;
                    }
                    return;
            }
        }

        private object GetImmediateParentScopeValue(ScriptThread thread)
        {
            try
            {
                return thread.CurrentScope.Parent.Values[SlotIndex];
            }
            catch { }
            try
            {
                return thread.CurrentScope.Parent.GetValue(SlotIndex);
            }
            catch { thread.CurrentNode = FromNode; throw; }
        }

        private object GetImmediateParentScopeParameter(ScriptThread thread)
        {
            try
            {
                return thread.CurrentScope.Parent.Parameters[SlotIndex];
            }
            catch { thread.CurrentNode = FromNode; throw; }
        }

        private void SetImmediateParentScopeValue(ScriptThread thread, object value, TypeInfo type)
        {
            CheckTypeMatch(thread, Slot.ValueType, type);
            thread.CurrentScope.Parent.SetValue(SlotIndex, value);
        }

        private void SetImmediateParentScopeParameter(ScriptThread thread, object value, TypeInfo type)
        {
            CheckTypeMatch(thread, Slot.ValueType, type);
            thread.CurrentScope.Parent.Parameters[SlotIndex] = value;
        }

        private object GetParentScopeValue(ScriptThread thread)
        {
            var targetScope = GetTargetScope(thread);
            return targetScope.GetValue(SlotIndex);
        }
        private object GetParentScopeParameter(ScriptThread thread)
        {
            var targetScope = GetTargetScope(thread);
            return targetScope.Parameters[SlotIndex];
        }
        private void SetParentScopeValue(ScriptThread thread, object value, TypeInfo type)
        {
            CheckTypeMatch(thread, Slot.ValueType, type);
            var targetScope = GetTargetScope(thread);
            targetScope.SetValue(SlotIndex, value);
        }
        private void SetParentScopeParameter(ScriptThread thread, object value, TypeInfo type)
        {
            CheckTypeMatch(thread, Slot.ValueType, type);
            var targetScope = GetTargetScope(thread);
            targetScope.Parameters[SlotIndex] = value;
        }
        private Scope GetTargetScope(ScriptThread thread)
        {
            var targetLevel = Slot.ScopeInfo.Level;
            var scope = thread.CurrentScope.Parent;
            while (scope.ScopeInfo.Level > targetLevel)
                scope = scope.Parent;
            return scope;
        }

        private void SetCurrentScopeParameter(ScriptThread thread, object value, TypeInfo type)
        {
            CheckTypeMatch(thread, Slot.ValueType, type);
            thread.CurrentScope.Parameters[SlotIndex] = value;
        }

        private void SetCurrentScopeValue(ScriptThread thread, object value, TypeInfo type)
        {
            CheckTypeMatch(thread, Slot.ValueType, type);
            thread.CurrentScope.SetValue(SlotIndex, value);
        }

        private object FastGetCurrentScopeParameter(ScriptThread thread)
        {
            try
            {
                return thread.CurrentScope.Parameters[SlotIndex];
            }
            catch
            {
                thread.CurrentNode = FromNode; throw;
            }
        }

        private object FastGetCurrentScopeValue(ScriptThread thread)
        {
            try
            {
                return thread.CurrentScope.Values[SlotIndex];
            }
            catch
            {
                return GetCurrentScopeValue(thread);
            }
        }

        private object GetCurrentScopeValue(ScriptThread thread)
        {
            try
            {
                return thread.CurrentScope.GetValue(SlotIndex);
            }
            catch
            {
                thread.CurrentNode = FromNode;
                throw;
            }
        }

        private void SetStatic(ScriptThread thread, object value, TypeInfo type)
        {
            CheckTypeMatch(thread, Slot.ValueType, type);
            thread.App.StaticScopes[StaticScopeIndex].SetValue(SlotIndex, value);
        }

        private object FastGetStaticValue(ScriptThread thread)
        {
            try
            {
                return thread.App.StaticScopes[StaticScopeIndex].Values[SlotIndex];
            }
            catch
            {
                return GetStaticValue(thread);
            }
        }

        private object GetStaticValue(ScriptThread thread)
        {
            try
            {
                return thread.App.StaticScopes[StaticScopeIndex].GetValue(SlotIndex);
            }
            catch
            {
                thread.CurrentNode = FromNode;
                throw;
            }
        }
        private static void CheckTypeMatch(ScriptThread thread, TypeInfo expectedType, TypeInfo actualType)
        {
            if (!Runtime.IsTypeMatch(expectedType, actualType))
            {
                thread.ThrowScriptError("Type mismatch! Expected {0} but got {1}", expectedType, actualType);
            }
        }
    }
}
