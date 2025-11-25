using System;
using Microsoft.AspNetCore.Mvc;
using DMS.Areas.Identity.Pages.Account;
using DMS.CommonUtil;
using DMS.Data;
using DMS.Models;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace DMS.Services
{
	public class AccountService
	{
        private readonly ApplicationDbContext _context;
        string connectionString = "Data Source=localhost:1521/FREEPDB1;User Id=JJC;Password=Qwer1234;";

        public AccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void RegisterService(T_Member member)
        {
            member.OID = Encryption.CreateRandomKey();
            member.TYPE = "MEMBER";
            member.CREATE_DATE = DateTime.Now;
            member.USEFLAG = '1';
            member.PASSWORD = BCrypt.Net.BCrypt.HashPassword(member.PASSWORD);

            _context.Objects.Add(member);
            _context.Members.Add(member);

            try
            {
                _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database Error: {dbEx.InnerException?.Message}");   
            }
        }

        // 아이디 확인
        public T_Member? CheckMemberID(string MEMBER_ID)
        {
            return _context.Members.SingleOrDefault(u => u.MEMBER_ID == MEMBER_ID);
        }


        // 맴버 확인
        public bool CheckUserCredentials(string MEMBER_ID, string PASSWORD)
        {
            // 데이터베이스에서 사용자 검색
            var member = _context.Members.SingleOrDefault(u => u.MEMBER_ID == MEMBER_ID);

            if (member != null && member.PASSWORD != null)
            {
                return VerifyPassword(member.PASSWORD, PASSWORD);
            }

            return false; // 사용자 없음
        }

        // 비밀번호 검증
        public bool VerifyPassword(string? storedHash, string enteredPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(storedHash))
                {
                    throw new ArgumentException("null값이거나 비어있습니다.");
                }

                if (!storedHash.StartsWith("$2a$") && !storedHash.StartsWith("$2b$") && !storedHash.StartsWith("$2y$"))
                {
                    throw new InvalidOperationException("유효한 해시가 아닙니다.");
                }

                bool isValid = BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);

                return isValid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }


        //public async Task EditService(string OID, T_Member member)
        //{
        //    // DB에서 기존 데이터 가져오기
        //    var existingMember = await _context.Members.SingleOrDefaultAsync(u => u.OID == OID);
        //    if (existingMember == null) return;

        //    // 필요한 값만 수정
        //    existingMember.NAME = member.NAME;
        //    existingMember.BIRTH_DATE = member.BIRTH_DATE;
        //    existingMember.RESIGN_DATE = member.RESIGN_DATE;
        //    existingMember.GENDER = member.GENDER;
        //    existingMember.EMAIL = member.EMAIL;

        //    // 비밀번호는 그대로 둠
        //    // existingMember.PASSWORD 유지

        //    // USEFLAG 강제 세팅
        //    existingMember.USEFLAG = '1';

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        Console.WriteLine($"Database Error: {dbEx.InnerException?.Message}");
        //    }
        //}


        //public async void EditService(string OID, T_Member member)
        //{
        //    _context.Entry(member).State = EntityState.Modified;
        //    var memberCheck = _context.Members.SingleOrDefault(u => u.OID == OID);
        //    //member.PASSWORD = BCrypt.Net.BCrypt.HashPassword(member.PASSWORD);
        //    member.PASSWORD = memberCheck.PASSWORD;
        //    member.USEFLAG = '1';

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException dbEx)
        //    {
        //        Console.WriteLine($"Database Error: {dbEx.InnerException?.Message}");
        //    }
        //}

        public bool EditService(string OID, [FromBody] T_Member member)
        {
            var memberCheck = _context.Members.SingleOrDefault(u => u.OID == OID);


            string query = @"
                        UPDATE JJC.OBJECT
                         SET NAME = :NAME,
                             MODIFY_DATE = :MODIFY_DATE
                         WHERE OID = :OID";

            //using (var connection = new OracleConnection(_context.Database.GetDbConnection().ConnectionString))

            try
            {
                using (OracleConnection connection = new OracleConnection(connectionString))
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(new OracleParameter("NAME", member.NAME));
                        command.Parameters.Add(new OracleParameter("MODIFY_DATE", DateTime.Now));
                        command.Parameters.Add(new OracleParameter("OID", OID));

                        int rowsAffected = command.ExecuteNonQuery();

                        //Console.WriteLine($"{rowsAffected} row(s) updated successfully.");
                    }

                    query = @"
                        UPDATE JJC.MEMBER
                         SET BIRTH_DATE = :BIRTHDATE,
                            RESIGN_DATE = :RESIGNDATE,
                            GENDER = :GENDER,
                            POSITION = :POSITION,
                            EMAIL = :EMAIL
                         WHERE OID = :OID";

                    using (OracleCommand command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add(new OracleParameter("BIRTHDATE", member.BIRTH_DATE));
                        command.Parameters.Add(new OracleParameter("RESIGNDATE", member.RESIGN_DATE));
                        command.Parameters.Add(new OracleParameter("GENDER", member.GENDER));
                        command.Parameters.Add(new OracleParameter("POSITION", member.POSITION));
                        command.Parameters.Add(new OracleParameter("EMAIL", member.EMAIL));
                        command.Parameters.Add(new OracleParameter("OID", OID));


                        int rowsAffected = command.ExecuteNonQuery();

                        Console.WriteLine($"{rowsAffected} row(s) updated successfully.");
                    }

                }

                return true;
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"Oracle Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }

        }


    }
}

