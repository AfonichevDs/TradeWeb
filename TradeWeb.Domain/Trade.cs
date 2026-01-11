using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeWeb.Domain;
public sealed record Trade(
    TradeDate Date,
    int ProductId,
    string Currency,
    decimal Price);