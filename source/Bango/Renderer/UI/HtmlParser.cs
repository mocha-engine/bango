﻿namespace Bango;
class HtmlParser : BaseParser
{
	public HtmlParser( string input ) : base( input )
	{
	}

	public string ParseTagName()
	{
		return ConsumeWhile( x => char.IsDigit( x ) || char.IsLetter( x ) );
	}

	public Node ParseNode()
	{
		ConsumeWhitespace();

		if ( NextChar() == '<' && !StartsWith( "</" ) )
			return ParseElement();
		else if ( char.IsLetter( NextChar() ) || char.IsDigit( NextChar() ) )
			return ParseText();

		return null;
	}

	public Node ParseText()
	{
		var textNode = new TextNode( ConsumeWhile( x => x != '<' ) );
		return textNode;
	}

	public Node ParseElement()
	{
		Assert( ConsumeChar() == '<' );
		var tagName = ParseTagName();
		var attributes = ParseAttributes();
		Assert( ConsumeChar() == '>' );

		var children = ParseNodes();

		Assert( ConsumeChar() == '<' );
		Assert( ConsumeChar() == '/' );
		Assert( ParseTagName() == tagName );
		Assert( ConsumeChar() == '>' );

		return new ElementNode( tagName, attributes, children );
	}

	public (string, string) ParseAttribute()
	{
		var name = ParseTagName();
		Assert( ConsumeChar() == '=' );
		var value = ParseAttributeValue();

		return (name, value);
	}

	public string ParseAttributeValue()
	{
		var openQuote = ConsumeChar();
		Assert( openQuote == '"' || openQuote == '\'' );
		var value = ConsumeWhile( x => x != openQuote );
		Assert( ConsumeChar() == openQuote );
		return value;
	}

	public Dictionary<string, string> ParseAttributes()
	{
		Dictionary<string, string> attributes = new();

		while ( NextChar() != '>' )
		{
			ConsumeWhitespace();
			var (name, value) = ParseAttribute();
			attributes.Add( name, value );
		}

		return attributes;
	}

	public List<Node> ParseNodes()
	{
		var nodes = new List<Node>();
		ConsumeWhitespace();

		while ( !EndOfFile() && !StartsWith( "</" ) )
		{
			var parsedNode = ParseNode();
			if ( parsedNode != null )
				nodes.Add( parsedNode );
		}

		return nodes;
	}

	public static Node Parse( string input )
	{
		var nodes = new HtmlParser( input ).ParseNodes();

		if ( nodes.Count == 1 )
		{
			return nodes[0];
		}
		else
		{
			return new ElementNode( "html", new(), nodes );
		}
	}
}
