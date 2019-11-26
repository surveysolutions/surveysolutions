using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Headquarters.Views.Device;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.OwinSecurity
{
    internal class HqUserStore : IUserRepository
    {
        private HQIdentityDbContext context;
        private readonly DbSet<HqUserLogin> _logins;
        private readonly DbSet<HqUserClaim> _userClaims;
        private readonly DbSet<HqUserRole> _userRoles;

        private readonly EntityStore<HqRole> _roleStore;
        private EntityStore<HqUser> _userStore;

        private bool _disposed;
        
        /// <summary>
        ///     If true will call SaveChanges after Create/Update/Delete
        /// </summary>
        public bool AutoSaveChanges { get; set; }

        /// <summary>
        ///     Returns an IQueryable of users
        /// </summary>
        public IQueryable<HqUser> Users => _userStore.EntitySet.Include("Profile").Include("Roles");

        public async Task<IList<string>> GetRolesAsync(Guid userId)
        {
            ThrowIfDisposed();

            var query = from userRole in _userRoles
                where userRole.UserId.Equals(userId)
                join role in _roleStore.DbEntitySet on userRole.RoleId equals role.Id
                select role.Name;
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetClaimsAsync(Guid userId)
        {
            ThrowIfDisposed();

            var user = await FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "UserId {0} not found.",
                    userId));
            }
            
            await EnsureClaimsLoadedAsync(user);
            return user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
        }

        public Task<string> GetEmailAsync(HqUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.Email);
        }

        public Task<HqUser> FindByEmailAsync(string email)
        {
            ThrowIfDisposed();
            return GetUserAggregateAsync(u => u.Email.ToUpper() == email.ToUpper());
        }

        public HqUserStore(HQIdentityDbContext context)
        {
            this.context = context;
            AutoSaveChanges = true;
            _userStore = new EntityStore<HqUser>(context);
            _roleStore = new EntityStore<HqRole>(context);
            _logins = context.Set<HqUserLogin>();
            _userClaims = context.Set<HqUserClaim>();
            _userRoles = context.Set<HqUserRole>();
        }

        public DbSet<DeviceSyncInfo> DeviceSyncInfos => context.DeviceSyncInfos;
        public Task<string> GetSecurityStampAsync(HqUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.SecurityStamp);
        }

        public async Task CreateAsync(HqUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            _userStore.Create(user);
            await SaveChanges();
        }

        public async Task UpdateAsync(HqUser user)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            _userStore.Update(user);
            await SaveChanges();
        }

        public Task<HqUser> FindByIdAsync(Guid userId)
        {
            ThrowIfDisposed();
            return GetUserAggregateAsync(u => u.Id.Equals(userId));
        }

        public HqUser FindById(Guid userId)
        {
            ThrowIfDisposed();
            return GetUserAggregate(u => u.Id.Equals(userId));
        }

        public Task<HqUser> FindByNameAsync(string userName)
        {
            ThrowIfDisposed();
            return GetUserAggregateAsync(u => u.UserName.ToUpper() == userName.ToUpper());
        }

        public Task SetPasswordHashAsync(HqUser user, string hash)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.PasswordHash = hash;
            return Task.FromResult(0);
        }

        public Task SetSecurityStampAsync(HqUser user, string newSecurityStamp)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.SecurityStamp = newSecurityStamp;
            return Task.FromResult(0);
        }

        public Task SetLockoutEnabledAsync(HqUser user, bool isLockoutEnabled)
        {
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.LockoutEnabled = isLockoutEnabled;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Used to attach child entities to the User aggregate, i.e. Roles, Logins, Claims and Profile
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        protected virtual async Task<HqUser> GetUserAggregateAsync(Expression<Func<HqUser, bool>> filter)
        {
            HqUser user;
            if (FindByIdFilterParser.TryMatchAndGetId(filter, out var id))
            {
                user = await _userStore.GetByIdAsync(id);
            }
            else
            {
                user = await Users.FirstOrDefaultAsync(filter);
            }
            if (user != null)
            {
                await EnsureClaimsLoadedAsync(user);
                await EnsureLoginsLoadedAsync(user);
                await EnsureRolesLoadedAsync(user);
                await EnsureProfileLoadedAsync(user);
            }
            return user;
        }

        /// <summary>
        /// Used to attach child entities to the User aggregate, i.e. Roles, Logins, Claims and Profile
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        protected virtual HqUser GetUserAggregate(Expression<Func<HqUser, bool>> filter)
        {
            HqUser user;
            if (FindByIdFilterParser.TryMatchAndGetId(filter, out var id))
            {
                user = _userStore.GetById(id);
            }
            else
            {
                user = Users.FirstOrDefault(filter);
            }
            if (user != null)
            {
                EnsureClaimsLoaded(user);
                EnsureLoginsLoaded(user);
                EnsureRolesLoaded(user);
                EnsureProfileLoaded(user);
            }
            return user;
        }

        // Only call save changes if AutoSaveChanges is true
        private async Task SaveChanges()
        {
            if (AutoSaveChanges)
            {
                await context.SaveChangesAsync();
            }
        }

        private async Task EnsureLoginsLoadedAsync(HqUser user)
        {
            if (!context.Entry(user).Collection(u => u.Logins).IsLoaded)
            {
                var userId = user.Id;
                await _logins.Where(uc => uc.UserId.Equals(userId)).LoadAsync();
                context.Entry(user).Collection(u => u.Logins).IsLoaded = true;
            }
        }

        private async Task EnsureClaimsLoadedAsync(HqUser user)
        {
            if (!context.Entry(user).Collection(u => u.Claims).IsLoaded)
            {
                var userId = user.Id;
                await _userClaims.Where(uc => uc.UserId.Equals(userId)).LoadAsync();
                context.Entry(user).Collection(u => u.Claims).IsLoaded = true;
            }
        }

        private async Task EnsureRolesLoadedAsync(HqUser user)
        {
            if (!context.Entry(user).Collection(u => u.Roles).IsLoaded)
            {
                var userId = user.Id;
                await _userRoles.Where(uc => uc.UserId.Equals(userId)).LoadAsync();
                context.Entry(user).Collection(u => u.Roles).IsLoaded = true;
            }
        }

        private async Task EnsureProfileLoadedAsync(HqUser user)
        {
            if (!context.Entry(user).Reference(x => x.Profile).IsLoaded)
                await context.Entry(user).Reference(x => x.Profile).LoadAsync();
        }

        private void EnsureLoginsLoaded(HqUser user)
        {
            if (!context.Entry(user).Collection(u => u.Logins).IsLoaded)
            {
                var userId = user.Id;
                _logins.Where(uc => uc.UserId.Equals(userId)).Load();
                context.Entry(user).Collection(u => u.Logins).IsLoaded = true;
            }
        }

        private void EnsureClaimsLoaded(HqUser user)
        {
            if (!context.Entry(user).Collection(u => u.Claims).IsLoaded)
            {
                var userId = user.Id;
                _userClaims.Where(uc => uc.UserId.Equals(userId)).Load();
                context.Entry(user).Collection(u => u.Claims).IsLoaded = true;
            }
        }

        private void EnsureRolesLoaded(HqUser user)
        {
            if (!context.Entry(user).Collection(u => u.Roles).IsLoaded)
            {
                var userId = user.Id;
                _userRoles.Where(uc => uc.UserId.Equals(userId)).Load();
                context.Entry(user).Collection(u => u.Roles).IsLoaded = true;
            }
        }

        private void EnsureProfileLoaded(HqUser user)
        {
            if (!context.Entry(user).Reference(x => x.Profile).IsLoaded)
                context.Entry(user).Reference(x => x.Profile).Load();
        }
        
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// <summary>
        ///     Dispose the store
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     If disposing, calls dispose on the Context.  Always nulls out the Context
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) context?.Dispose();

            _disposed = true;
            context = null;
            _userStore = null;
        }

        // We want to use FindAsync() when looking for an User.Id instead of LINQ to avoid extra 
        // database roundtrips. This class cracks open the filter expression passed by 
        // UserStore.FindByIdAsync() to obtain the value of the id we are looking for 
        private static class FindByIdFilterParser
        {
            // expression pattern we need to match
            private static readonly Expression<Func<HqUser, bool>> Predicate = u => u.Id.Equals(default(Guid));
            // method we need to match: Object.Equals() 
            private static readonly MethodInfo EqualsMethodInfo = ((MethodCallExpression)Predicate.Body).Method;
            // property access we need to match: User.Id 
            private static readonly MemberInfo UserIdMemberInfo = ((MemberExpression)((MethodCallExpression)Predicate.Body).Object).Member;

            internal static bool TryMatchAndGetId(Expression<Func<HqUser, bool>> filter, out Guid id)
            {
                // default value in case we can’t obtain it 
                id = default(Guid);

                // lambda body should be a call 
                if (filter.Body.NodeType != ExpressionType.Call)
                {
                    return false;
                }

                // actually a call to object.Equals(object)
                var callExpression = (MethodCallExpression)filter.Body;
                if (callExpression.Method != EqualsMethodInfo)
                {
                    return false;
                }
                // left side of Equals() should be an access to User.Id
                if (callExpression.Object == null
                    || callExpression.Object.NodeType != ExpressionType.MemberAccess
                    || ((MemberExpression)callExpression.Object).Member != UserIdMemberInfo)
                {
                    return false;
                }

                // There should be only one argument for Equals()
                if (callExpression.Arguments.Count != 1)
                {
                    return false;
                }

                MemberExpression fieldAccess;
                if (callExpression.Arguments[0].NodeType == ExpressionType.Convert)
                {
                    // convert node should have an member access access node
                    // This is for cases when primary key is a value type
                    var convert = (UnaryExpression)callExpression.Arguments[0];
                    if (convert.Operand.NodeType != ExpressionType.MemberAccess)
                    {
                        return false;
                    }
                    fieldAccess = (MemberExpression)convert.Operand;
                }
                else if (callExpression.Arguments[0].NodeType == ExpressionType.MemberAccess)
                {
                    // Get field member for when key is reference type
                    fieldAccess = (MemberExpression)callExpression.Arguments[0];
                }
                else
                {
                    return false;
                }

                // and member access should be a field access to a variable captured in a closure
                if (fieldAccess.Member.MemberType != MemberTypes.Field
                    || fieldAccess.Expression.NodeType != ExpressionType.Constant)
                {
                    return false;
                }

                // expression tree matched so we can now just get the value of the id 
                var fieldInfo = (FieldInfo)fieldAccess.Member;
                var closure = ((ConstantExpression)fieldAccess.Expression).Value;

                id = (Guid)fieldInfo.GetValue(closure);
                return true;
            }
        }
    }

    /// <summary>
    ///     EntityFramework based IIdentityEntityStore that allows query/manipulation of a TEntity set
    /// </summary>
    /// <typeparam name="TEntity">Concrete entity type, i.e .User</typeparam>
    internal class EntityStore<TEntity> where TEntity : class
    {
        /// <summary>
        ///     Constructor that takes a Context
        /// </summary>
        /// <param name="context"></param>
        public EntityStore(DbContext context)
        {
            Context = context;
            DbEntitySet = context.Set<TEntity>();
        }

        /// <summary>
        ///     Context for the store
        /// </summary>
        public DbContext Context { get; private set; }

        /// <summary>
        ///     Used to query the entities
        /// </summary>
        public IQueryable<TEntity> EntitySet => DbEntitySet;

        /// <summary>
        ///     EntitySet for this store
        /// </summary>
        public DbSet<TEntity> DbEntitySet { get; private set; }

        /// <summary>
        ///     FindAsync an entity by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual ValueTask<TEntity> GetByIdAsync(object id) => DbEntitySet.FindAsync(id);

        /// <summary>
        ///     FindAsync an entity by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual TEntity GetById(object id) => DbEntitySet.Find(id);

        /// <summary>
        ///     Insert an entity
        /// </summary>
        /// <param name="entity"></param>
        public void Create(TEntity entity) => DbEntitySet.Add(entity);

        /// <summary>
        ///     Mark an entity for deletion
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(TEntity entity) => DbEntitySet.Remove(entity);

        /// <summary>
        ///     Update an entity
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Update(TEntity entity)
        {
            if (entity != null)
            {
                Context.Entry(entity).State = EntityState.Modified;
            }
        }
    }
}
