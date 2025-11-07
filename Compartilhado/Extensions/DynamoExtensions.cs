using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ModelAV = Amazon.DynamoDBv2.Model.AttributeValue;
using EventAV = Amazon.Lambda.DynamoDBEvents.DynamoDBEvent.AttributeValue;

namespace Compartilhado.Extensions;

public static class DynamoExtensions
{
    // Extensão para o tipo do EVENTO (NewImage)
    public static T ToObject<T>(this IDictionary<string, EventAV> map)
    {
        var modelMap = map.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToModelAttributeValue()
        );

        var doc = Document.FromAttributeMap(modelMap);

        using var client = new AmazonDynamoDBClient();
        var context = new DynamoDBContext(client);
        return context.FromDocument<T>(doc);
    }

    // Conversor de AttributeValue do EVENTO -> MODEL
    public static ModelAV ToModelAttributeValue(this EventAV av)
    {
        var mav = new ModelAV
        {
            S = av.S,
            N = av.N,
            B = av.B,
            NULL = av.NULL,
            BOOL = av.BOOL
        };

        if (av.SS != null) 
            mav.SS = av.SS;

        if (av.NS != null) 
            mav.NS = av.NS;
        
        if (av.BS != null) 
            mav.BS = av.BS;

        if (av.M != null)
            mav.M = av.M.ToDictionary(p => p.Key, p => p.Value.ToModelAttributeValue());

        if (av.L != null)
            mav.L = av.L.Select(ToModelAttributeValue).ToList();

        return mav;
    }
}