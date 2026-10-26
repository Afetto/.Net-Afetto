using Afetto_.Net.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Afetto_.Net.Hateoas
{
    /// <summary>Monta os links HATEOAS dos endpoints de consulta de usuários.</summary>
    public static class UsuarioLinkBuilder
    {
        private const string Controller = "Usuario";

        /// <summary>Links do recurso: self, update e delete.</summary>
        public static void AdicionarLinks(this UsuarioResponse usuario, IUrlHelper url, string scheme)
        {
            var valores = new { id = usuario.Id };
            usuario.Links = new List<LinkDto>
            {
                new() { Rel = "self",   Method = "GET",    Href = url.Action("GetById", Controller, valores, scheme)! },
                new() { Rel = "update", Method = "PUT",    Href = url.Action("Update",  Controller, valores, scheme)! },
                new() { Rel = "delete", Method = "DELETE", Href = url.Action("Delete",  Controller, valores, scheme)! }
            };
        }

        public static void AdicionarLinks(this IEnumerable<UsuarioResponse> usuarios, IUrlHelper url, string scheme)
        {
            foreach (var u in usuarios)
                u.AdicionarLinks(url, scheme);
        }

        /// <summary>Links da coleção paginada: self, first, prev, next e last (preservando filtros).</summary>
        public static void AdicionarLinks(
            this PagedResponse<UsuarioResponse> pagina,
            IUrlHelper url,
            string scheme,
            UsuarioQueryParameters q)
        {
            string Href(int page) => url.Action("GetAll", Controller, new
            {
                page,
                pageSize = q.PageSize,
                sortBy = q.SortBy,
                sortDir = q.SortDir,
                nome = q.Nome,
                email = q.Email,
                cidadeId = q.CidadeId,
                logradouroId = q.LogradouroId
            }, scheme)!;

            var ultima = Math.Max(pagina.TotalPages, 1);

            pagina.Links = new List<LinkDto>
            {
                new() { Rel = "self",  Href = Href(pagina.Page) },
                new() { Rel = "first", Href = Href(1) },
                new() { Rel = "last",  Href = Href(ultima) }
            };

            if (pagina.Page > 1)
                pagina.Links.Add(new LinkDto { Rel = "prev", Href = Href(pagina.Page - 1) });

            if (pagina.Page < pagina.TotalPages)
                pagina.Links.Add(new LinkDto { Rel = "next", Href = Href(pagina.Page + 1) });
        }
    }
}
