#nullable disable
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using PangyaAPI.Utilities.Resources;

namespace PangyaAPI.Utilities
{
    public static class ExceptionError
    {
        public static uint STDA_SOURCE_ERROR_ENCODE(uint source_error) => (uint)(((source_error) << 24) & 0xFF000000);
        public static uint STDA_SOURCE_ERROR_DECODE(uint err_code) => (uint)(((err_code) >> 24) & 0x000000FF);
        public static STDA_ERROR_TYPE STDA_SOURCE_ERROR_DECODE_TYPE(uint err_code) => (STDA_ERROR_TYPE)(((err_code) >> 24) & 0x000000FF);
        public static uint STDA_ERROR_ENCODE(uint err) => (uint)(((err) << 16) & 0x00FF0000);
        public static uint STDA_ERROR_DECODE(uint err_code) => (uint)(((err_code) >> 16) & 0x000000FF);
        public static uint STDA_SYSTEM_ERROR_ENCODE(uint _err_sys) => (uint)((_err_sys) & 0x0000FFFF);
        public static uint STDA_SYSTEM_ERROR_DECODE(uint err_code) => (uint)((err_code) & 0x0000FFFF);
        public static uint STDA_SYSTEM_ERROR_DECODE_TYPE(uint err_code) => (uint)((err_code) & 0x0000FFFF);
        public static uint STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE source_error, uint err_code, uint _err_sys) => (uint)(STDA_SOURCE_ERROR_ENCODE(Convert.ToUInt32(source_error)) | STDA_ERROR_ENCODE((err_code)) | STDA_SYSTEM_ERROR_ENCODE((_err_sys)));
        public static uint STDA_MAKE_ERROR(uint source_error, uint err_code, uint _err_sys) => (uint)(STDA_SOURCE_ERROR_ENCODE((source_error)) | STDA_ERROR_ENCODE((err_code)) | STDA_SYSTEM_ERROR_ENCODE((_err_sys)));
        public static bool STDA_ERROR_CHECK_SOURCE_AND_ERROR(uint err_code, uint source_error, uint error) => (bool)(STDA_SOURCE_ERROR_DECODE((err_code)) == (source_error) && STDA_ERROR_DECODE((err_code)) == (error));
        public static bool STDA_ERROR_CHECK_SOURCE_AND_ERROR_TYPE(uint err_code, STDA_ERROR_TYPE source_error, uint error) => (bool)(STDA_SOURCE_ERROR_DECODE((err_code)) == (uint)(source_error) && STDA_ERROR_DECODE((err_code)) == (error));

    }
}
