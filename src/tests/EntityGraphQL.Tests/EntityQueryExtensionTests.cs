using Xunit;
using System.Collections.Generic;
using System.Linq;
using static EntityGraphQL.Schema.ArgumentHelper;
using EntityGraphQL.Extensions;
using EntityGraphQL.Schema;
using EntityGraphQL.Compiler;
using System;
using EntityGraphQL.LinqQuery;

namespace EntityGraphQL.Tests
{
    public class EntityQueryExtensionTests
    {
        [Fact]
        public async void SupportEntityQuery()
        {
            var schemaProvider = SchemaBuilder.FromObject<TestSchema>(false);
            schemaProvider.ReplaceField("users", new { filter = EntityQuery<User>() }, (ctx, p) => ctx.Users.Where(p.filter), "Return filtered users");
            var tree = new GraphQLCompiler(schemaProvider, new DefaultMethodProvider()).Compile(@"query {
	users(filter: ""field2 = ""2"" "") { field2 }
}").Operations.First();
            Assert.Single(tree.QueryFields);
            dynamic result = await tree.ExecuteAsync(new TestSchema(), new GraphQLValidator(), null);
            Assert.Equal(1, Enumerable.Count(result.users));
            var user = Enumerable.First(result.users);
            Assert.Equal("2", user.field2);
        }

        [Fact]
        public void SupportEntityQueryArgument()
        {
            var schemaProvider = SchemaBuilder.FromObject<TestSchema>(false);
            schemaProvider.ReplaceField("users", new { filter = EntityQuery<User>() }, (ctx, p) => ctx.Users.Where(p.filter), "Return filtered users");
            var gql = new QueryRequest
            {
                Query = @"query {
                    users(filter: $filter) { field2 }
                }",
                Variables = new QueryVariables { { "filter", "field2 = \"2\"" } }
            };
            var tree = schemaProvider.ExecuteQuery(gql, new TestSchema(), null, null);
            dynamic users = ((IDictionary<string, object>)tree.Data)["users"];
            Assert.Equal(1, Enumerable.Count(users));
            var user = Enumerable.First(users);
            Assert.Equal("2", user.field2);
        }
    }
}