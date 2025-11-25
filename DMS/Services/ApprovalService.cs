using System;
using DMS.Data;
using DMS.Models;
using Microsoft.EntityFrameworkCore;
using DMS.CommonUtil;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace DMS.Services
{
    public class ApprovalService
    {
        private readonly ApplicationDbContext _context;

        public ApprovalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<T_Approval>> GetApprovals()
        {
            return await _context.Approvals.ToListAsync();
        }

        public async Task AddApprovalAsync(T_Approval approval)
        {
            approval.OID = Encryption.CreateRandomKey();
            approval.TYPE = "APPROVAL";
            //approval.CREATE_DATE = DateTime.Now;
            approval.STATE = "결재중";
            approval.USEFLAG = '1';
            _context.Approvals.Add(approval);
            await _context.SaveChangesAsync();
        }

    }
}
