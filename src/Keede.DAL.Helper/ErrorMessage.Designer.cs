﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.18444
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Keede.DAL.Helper
{
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ErrorMessage {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ErrorMessage() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Framework.Data.ErrorMessage", typeof(ErrorMessage).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   使用此强类型资源类，为所有资源查找
        ///   重写当前线程的 CurrentUICulture 属性。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 Prepare 的本地化字符串。
        /// </summary>
        internal static string ConcurrentDictionaryCache_Prepare_Prepare {
            get {
                return ResourceManager.GetString("ConcurrentDictionaryCache_Prepare_Prepare", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 非法数据库连接串描述! 的本地化字符串。
        /// </summary>
        internal static string CONNECTION_STRING_ERROR {
            get {
                return ResourceManager.GetString("CONNECTION_STRING_ERROR", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 {0}.{1}，发生错误：{2} 的本地化字符串。
        /// </summary>
        internal static string METHOD_EXCEPTION {
            get {
                return ResourceManager.GetString("METHOD_EXCEPTION", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 {0}字段值到{1}属性转换错误! 的本地化字符串。
        /// </summary>
        internal static string READER_TO_PROPERTY_ERROR {
            get {
                return ResourceManager.GetString("READER_TO_PROPERTY_ERROR", resourceCulture);
            }
        }
    }
}
