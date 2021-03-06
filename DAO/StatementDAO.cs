﻿using br.com.sagradacruz.Connection;
using br.com.sagradacruz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace br.com.sagradacruz.DAO
{
    public class StatementDAO
    {
        Database _db = new Database();

        public List<StatementViewModel> GetStatements(bool onlyPublished=false)
        {
            var statements = new List<StatementViewModel>();
            var conn = _db.OpenConnection();
            try
            {
                var command = conn.CreateCommand();
                var cmdSql = new StringBuilder();

                cmdSql.AppendLine("SELECT id, creation_date");
                cmdSql.AppendLine(",author");
                cmdSql.AppendLine(",city");
                cmdSql.AppendLine(",content");
                cmdSql.AppendLine(",state");
                cmdSql.AppendLine("FROM sagradacruz.statement");

                if (onlyPublished)
                {
                    cmdSql.AppendLine("WHERE state = 'P'");
                }

                cmdSql.AppendLine("ORDER BY 1 DESC");

                command.CommandText = cmdSql.ToString();

                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    statements.Add(new StatementViewModel {
                        Id = reader.GetInt32("id"),
                        Author = reader.GetString("author") ?? "ANÔNIMO",
                        City = reader.GetString("city") ?? "ANÔNIMO",
                        Content = reader.GetString("content") ?? "SEM CONTEÚDO",
                        CreationDate = reader.GetDateTime("creation_date"),
                        Status = reader.GetString("state") ?? "R"
                    });
                }

                return statements;
            }
            catch (Exception)
            {
                return new List<StatementViewModel>();
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        public bool CreateStatement(StatementViewModel statement)
        {
            var conn = _db.OpenConnection();
            try
            {
                var command = conn.CreateCommand();
                var cmdSql = @"INSERT INTO sagradacruz.statement
                               (author, creation_date, city, content, state)
                               VALUES 
                               (@author, current_date(), @city, @content, 'W')";

                command.CommandText = cmdSql;

                _db.ClearParameters(ref command);
                _db.AddInParameter(ref command, "@author", statement.Author);
                _db.AddInParameter(ref command, "@city", statement.City);
                _db.AddInParameter(ref command, "@content", statement.Content);

                var result = command.ExecuteNonQuery();

                return (result > 0);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        public bool ApproveStatement(int id, bool approved)
        {            
            var conn = _db.OpenConnection();

            try
            {
                var command = conn.CreateCommand();
                var cmdSql = @"UPDATE sagradacruz.statement
                               SET state = @state
                               WHERE id = @id";

                command.CommandText = cmdSql;

                _db.ClearParameters(ref command);
                _db.AddInParameter(ref command, "@id", id);
                _db.AddInParameter(ref command, "@state", (approved ? 'P' : 'R')); // P - Publicado / R - Rejeitado

                var result = command.ExecuteNonQuery();

                return (result > 0);
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }
}
