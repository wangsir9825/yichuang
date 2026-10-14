using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.Script.Serialization;

internal static class RebuiltProxy
{
    private const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance;
    private static readonly Dictionary<short,OpCode> Opcodes=new Dictionary<short,OpCode>();
    static RebuiltProxy() {
        foreach(FieldInfo f in typeof(OpCodes).GetFields(BindingFlags.Public|BindingFlags.Static)) {
            if(f.FieldType==typeof(OpCode)) { OpCode op=(OpCode)f.GetValue(null); Opcodes[op.Value]=op; }
        }
    }
    static Type T(object name) { return Type.GetType((string)name,true); }
    static object Resolve(Dictionary<string,object> d) {
        string kind=(string)d["kind"];
        if(kind=="string")return (string)d["value"];
        if(kind=="signature")return Convert.FromBase64String((string)d["value"]);
        Type type=T(d["type"]);
        if(kind=="type")return type;
        if(kind=="field")return type.GetField((string)d["name"],Flags);
        string name=(string)d["name"];
        IList p=(IList)d["parameters"];
        Type[] parameters=new Type[p.Count];for(int i=0;i<p.Count;i++)parameters[i]=T(p[i]);
        IList g=(IList)d["generic"];
        Type[] generic=new Type[g.Count];for(int i=0;i<g.Count;i++)generic[i]=T(g[i]);
        if(name==".ctor")return type.GetConstructor(Flags & ~BindingFlags.Static,null,parameters,null);
        if(name==".cctor")return type.TypeInitializer;
        foreach(MethodInfo candidate in type.GetMethods(Flags)) {
            if(candidate.Name!=name)continue;
            MethodInfo method=candidate;
            if(generic.Length>0) {
                if(!candidate.IsGenericMethodDefinition||candidate.GetGenericArguments().Length!=generic.Length)continue;
                method=candidate.MakeGenericMethod(generic);
            } else if(candidate.IsGenericMethodDefinition)continue;
            if(d.ContainsKey("returnType") && d["returnType"]!=null && method.ReturnType!=T(d["returnType"]))continue;
            ParameterInfo[] actual=method.GetParameters();if(actual.Length!=parameters.Length)continue;
            bool match=true;for(int i=0;i<actual.Length;i++)if(actual[i].ParameterType!=parameters[i])match=false;
            if(match)return method;
        }
        throw new MissingMethodException(type.FullName,name);
    }
    static int Token(DynamicILInfo info, object value) {
        if(value==null)throw new InvalidDataException("Missing proxy token");
        Type type=value as Type;if(type!=null)return info.GetTokenFor(type.TypeHandle);
        MethodBase method=value as MethodBase;if(method!=null)return info.GetTokenFor(method.MethodHandle,method.DeclaringType.TypeHandle);
        FieldInfo field=value as FieldInfo;if(field!=null)return info.GetTokenFor(field.FieldHandle,field.DeclaringType.TypeHandle);
        if(value is string)return info.GetTokenFor((string)value);
        if(value is byte[])return info.GetTokenFor((byte[])value);
        throw new NotSupportedException(value.GetType().FullName);
    }
    static void Relocate(byte[] code, DynamicILInfo info, IList tokens) {
        int cursor=0;
        while(cursor<code.Length) {
            byte first=code[cursor++];short key=first==0xfe ? unchecked((short)(0xfe00|code[cursor++])) : (short)first;
            OpCode op=Opcodes[key];int length=0;
            switch(op.OperandType) {
                case OperandType.InlineNone:break;
                case OperandType.ShortInlineBrTarget:case OperandType.ShortInlineI:case OperandType.ShortInlineVar:length=1;break;
                case OperandType.InlineVar:length=2;break;
                case OperandType.InlineI8:case OperandType.InlineR:length=8;break;
                case OperandType.InlineSwitch:length=4+4*BitConverter.ToInt32(code,cursor);break;
                case OperandType.InlineMethod:case OperandType.InlineField:case OperandType.InlineType:case OperandType.InlineTok:case OperandType.InlineSig:case OperandType.InlineString:
                    int old=BitConverter.ToInt32(code,cursor)&0x00ffffff;
                    int token=Token(info,Resolve((Dictionary<string,object>)tokens[old]));
                    Buffer.BlockCopy(BitConverter.GetBytes(token),0,code,cursor,4);length=4;break;
                default:length=4;break;
            }
            cursor+=length;
        }
    }
    internal static object[] Create() {
        string json;
        using(Stream stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("Rebuild.delegates.json"))
        using(StreamReader reader=new StreamReader(stream))json=reader.ReadToEnd();
        JavaScriptSerializer serializer=new JavaScriptSerializer();serializer.MaxJsonLength=32*1024*1024;
        object[] entries=serializer.Deserialize<object[]>(json);
        object[] result=new object[797];
        foreach(Dictionary<string,object> entry in entries) {
            int index=(int)entry["index"];
            string name=(string)entry["delegateType"];
            if(char.IsDigit(name[0]))name="_"+name;
            Type dtype=Assembly.GetExecutingAssembly().GetType(name,true);
            try {
                if(entry.ContainsKey("direct")) {
                    result[index]=Delegate.CreateDelegate(dtype,(MethodInfo)Resolve((Dictionary<string,object>)entry["direct"]));
                } else {
                    MethodInfo invoke=dtype.GetMethod("Invoke");ParameterInfo[] p=invoke.GetParameters();
                    Type[] parameters=new Type[p.Length];for(int i=0;i<p.Length;i++)parameters[i]=p[i].ParameterType;
                    DynamicMethod method=new DynamicMethod("RebuiltProxy_"+index,invoke.ReturnType,parameters,typeof(RebuiltProxy).Module,true);
                    DynamicILInfo info=method.GetDynamicILInfo();byte[] code=Convert.FromBase64String((string)entry["code"]);
                    Relocate(code,info,(IList)entry["tokens"]);
                    info.SetLocalSignature(new byte[]{7,0});info.SetCode(code,(int)entry["maxStack"]);
                    result[index]=method.CreateDelegate(dtype);
                }
            } catch(Exception ex) { throw new InvalidOperationException("Could not restore proxy "+index+" ("+name+")",ex); }
        }
        return result;
    }
}

internal static class _003CPrivateImplementationDetails_003E
{
    internal static uint ComputeStringHash(string value) {
        uint hash=2166136261;
        if(value!=null)for(int i=0;i<value.Length;i++)hash=unchecked((hash^value[i])*16777619);
        return hash;
    }
}
